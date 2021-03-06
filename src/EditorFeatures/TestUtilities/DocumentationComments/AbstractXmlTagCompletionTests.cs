﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.UnitTests.Utilities;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Operations;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.DocumentationComments
{
    public abstract class AbstractXmlTagCompletionTests
    {
        internal abstract IChainedCommandHandler<TypeCharCommandArgs> CreateCommandHandler(ITextUndoHistoryRegistry undoHistory);
        protected abstract TestWorkspace CreateTestWorkspace(string initialMarkup);

        public void Verify(string initialMarkup, string expectedMarkup, char typeChar)
        {
            using (var workspace = CreateTestWorkspace(initialMarkup))
            {
                var testDocument = workspace.Documents.Single();
                var view = testDocument.GetTextView();
                view.Caret.MoveTo(new SnapshotPoint(view.TextSnapshot, testDocument.CursorPosition.Value));

                var commandHandler = CreateCommandHandler(workspace.GetService<ITextUndoHistoryRegistry>());

                var args = new TypeCharCommandArgs(view, view.TextBuffer, typeChar);
                var nextHandler = CreateInsertTextHandler(view, typeChar.ToString());

                commandHandler.ExecuteCommand(args, nextHandler, TestCommandExecutionContext.Create());
                MarkupTestFile.GetPosition(expectedMarkup, out var expectedCode, out int expectedPosition);

                Assert.Equal(expectedCode, view.TextSnapshot.GetText());

                var caretPosition = view.Caret.Position.BufferPosition.Position;
                Assert.True(expectedPosition == caretPosition,
                    string.Format("Caret positioned incorrectly. Should have been {0}, but was {1}.", expectedPosition, caretPosition));
            }
        }

        private Action CreateInsertTextHandler(ITextView textView, string text)
        {
            return () =>
            {
                var caretPosition = textView.Caret.Position.BufferPosition;
                var newSpanshot = textView.TextBuffer.Insert(caretPosition, text);
                textView.Caret.MoveTo(new SnapshotPoint(newSpanshot, caretPosition + text.Length));
            };
        }
    }
}
