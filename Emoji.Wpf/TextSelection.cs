//
//  Emoji.Wpf — Emoji support for WPF
//
//  Copyright © 2017–2022 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
#if DEBUG
using System.Text.RegularExpressions;
#endif
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Emoji.Wpf
{
    public sealed class TextSelection : TextRange
    {
        public TextSelection(TextPointer start, TextPointer end)
          : base(start, end) { }

        /// <summary>
        /// Override selection to text conversion in order to convert back all
        /// EmojiInline instances to their equivalent UTF-8 sequences.
        /// </summary>
        public new string Text
        {
            get
            {
                var buf = new StringBuilder();
                var is_first_paragraph = true;

                for (TextPointer p = Start, next = null;
                     p != null && p.CompareTo(End) < 0;
                     p = next)
                {
                    next = p.GetNextContextPosition(LogicalDirection.Forward);
                    if (next == null)
                        break;

                    switch (p.GetPointerContext(LogicalDirection.Forward))
                    {
                        case TextPointerContext.ElementStart:
                            var element = p.GetAdjacentElement(LogicalDirection.Forward);
                            if (element is EmojiInline emoji)
                                buf.Append(emoji.Text);
                            else if (element is Paragraph && !is_first_paragraph)
                                buf.Append('\n');
                            break;

                        case TextPointerContext.ElementEnd:
                            if (p.GetAdjacentElement(LogicalDirection.Forward) is Paragraph)
                                is_first_paragraph = false;
                            break;

                        case TextPointerContext.EmbeddedElement:
                            break;

                        case TextPointerContext.Text:
                            // Get text from the Run but don’t go past end
                            buf.Append(new TextRange(p, next.CompareTo(End) < 0 ? next : End).Text);
                            break;
                    }
                }

                return buf.ToString();
            }
        }
    }
}
