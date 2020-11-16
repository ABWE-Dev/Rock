﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;

namespace Rock.Lava
{
    public class LavaTagInfo : ILavaElementInfo
    {
        public LavaTagInfo()
        {
            //
        }

        public LavaTagInfo( string name, string systemTypeName )
        {
            Name = name;
            SystemTypeName = systemTypeName;
        }

        public string Name { get; set; }

        public string SystemTypeName { get; set; }

        /// <summary>
        /// The factory method used to create a new instance of this tag.
        /// </summary>
        public Func<string, IRockLavaTag> FactoryMethod { get; set; }

        /// <summary>
        /// Can the factory method successfully produce an instance of this tag?
        /// </summary>
        public bool IsAvailable { get; set; }

        public LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return LavaShortcodeTypeSpecifier.Inline;
            }
        }

        public override string ToString()
        {
            return string.Format( "{0} [{1}]", Name, SystemTypeName );
        }
    }

}