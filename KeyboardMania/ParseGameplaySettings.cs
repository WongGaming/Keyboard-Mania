using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KeyboardMania
{
    internal class ParseGameplaySettings
    {
        private ContentManager _content;

        public ParseGameplaySettings(ContentManager content)
        {
            _content = content;
        }
    }
}
