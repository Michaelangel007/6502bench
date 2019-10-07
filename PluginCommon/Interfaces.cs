﻿/*
 * Copyright 2018 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;

namespace PluginCommon {
    /// <summary>
    /// Extension script "plugins" must implement this interface.
    /// </summary>
    public interface IPlugin {
        /// <summary>
        /// Identification string.  Contents are arbitrary, but should briefly identify the
        /// purpose of the plugin, e.g. "Apple II ProDOS 8 MLI call handler".  It may
        /// contain version information, but should not be expected to be machine-readable.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Prepares the plugin for action.  Called at the start of the code analysis pass.
        /// 
        /// In the current implementation, the file data will be the same every time,
        /// because it doesn't change after the project is opened.  However, this could
        /// change if we add a descramble feature.
        /// </summary>
        /// <param name="appRef">Reference to application interface.</param>
        /// <param name="fileData">65xx code and data.</param>
        /// <param name="addrMap">Mapping between offsets and addresses.</param>
        /// <param name="plSyms">Symbols available to plugins, in no particular order.  All
        ///   platform, project, and user labels are included; auto-generated symbols and
        ///   local variables are not.</param>
        void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans,
            List<PlSymbol> plSyms);
    }

    /// <summary>
    /// Extension scripts that want to handle inline JSRs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineJsr {
        /// <summary>
        /// Checks to see if code/data near a JSR instruction should be formatted.
        ///
        /// The file data is guaranteed to hold all bytes of the JSR (offset + 2).
        /// </summary>
        /// <param name="offset">Offset of the JSR instruction.</param>
        /// <param name="noContinue">Set to true if the JSR doesn't actually return.</param>
        void CheckJsr(int offset, out bool noContinue);
    }

    /// <summary>
    /// Extension scripts that want to handle inline JSLs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineJsl {
        /// <summary>
        /// Checks to see if code/data near a JSL instruction should be formatted.
        ///
        /// The file data is guaranteed to hold all bytes of the JSL (offset + 3).
        /// </summary>
        /// <param name="offset">Offset of the JSL instruction.</param>
        /// <param name="noContinue">Set to true if the JSL doesn't actually return.</param>
        void CheckJsl(int offset, out bool noContinue);
    }

    /// <summary>
    /// Extension scripts that want to handle inline BRKs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineBrk {
        /// <summary>
        /// Checks to see if code/data near a BRK instruction should be formatted.
        ///
        /// The file data is only guaranteed to hold the BRK opcode byte.
        /// </summary>
        /// <param name="offset">Offset of the BRK instruction.</param>
        /// <param name="noContinue">Set to true if the BRK doesn't actually return.</param>
        void CheckBrk(int offset, out bool noContinue);
    }

    /// <summary>
    /// Interfaces provided by the application for use by plugins.  An IApplication instance
    /// is passed to the plugin as an argument Prepare().
    /// </summary>
    public interface IApplication {
        /// <summary>
        /// Sends a debug message to the application.  This can be useful when debugging scripts.
        /// (Use DEBUG > Show Analyzer Output to view it.)
        /// </summary>
        /// <param name="msg">Message to send.</param>
        void DebugLog(string msg);

        /// <summary>
        /// Specifies operand formatting.
        /// </summary>
        /// <param name="offset">File offset of opcode.</param>
        /// <param name="subType">Sub-type.  Must be appropriate for NumericLE.</param>
        /// <param name="label">Optional symbolic label.</param>
        /// <returns>True if the change was made, false if it was rejected.</returns>
        bool SetOperandFormat(int offset, DataSubType subType, string label);

        /// <summary>
        /// Formats file data as inline data.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="length">Length of item.</param>
        /// <param name="type">Type of item.  Must be NumericLE, NumericBE, or Dense.</param>
        /// <param name="subType">Sub-type.  Must be appropriate for type.</param>
        /// <param name="label">Optional symbolic label.</param>
        /// <returns>True if the change was made, false if it was rejected (e.g. because
        ///   the area is already formatted, or contains code).</returns>
        /// <exception cref="PluginException">If something is really wrong, e.g. data runs
        ///   off end of file.</exception>
        bool SetInlineDataFormat(int offset, int length, DataType type,
                DataSubType subType, string label);
    }

    /// <summary>
    /// Data format type.
    /// </summary>
    public enum DataType {
        Unknown = 0,
        NumericLE,
        NumericBE,
        StringGeneric,
        StringReverse,
        StringNullTerm,
        StringL8,
        StringL16,
        StringDci,
        Dense,
        Fill
    }

    /// <summary>
    /// Data format sub-type.
    /// </summary>
    public enum DataSubType {
        // No sub-type specified.
        None = 0,

        // For NumericLE/BE
        Hex,
        Decimal,
        Binary,
        Address,
        Symbol,

        // Strings and NumericLE/BE (single character)
        Ascii,
        HighAscii,
        C64Petscii,
        C64Screen
    }
}
