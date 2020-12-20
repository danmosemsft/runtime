//---------------------------------------------------------------------------
//
// <copyright file="SpeechObject.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description:     Helper class that encapsulates the basic functionality
//                  of a speakable element. Derived classed implement control-
//                  specific behavior.
//
// History:
//		2/1/2005	philsch     Created initial version
//---------------------------------------------------------------------------

#region Using directives

using System;
using System.IO;
using System.Resources;
using System.Diagnostics;
using System.Globalization;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Automation.Provider;

using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
#endregion

namespace System.Speech.Internal
{
    #region Base class: SpeechObject
    // base class
    internal abstract class SpeechObject
    {
        //*******************************************************************
        //
        // Protected Methods
        //
        //*******************************************************************

        #region Protected Methods
        protected void Initialize(DependencyObject element, string text, SrgsOneOf oneof, int id)
        {
            _element = element;
            _id = id;

            // add it to the speech grammar
            SrgsItem item = new SrgsItem();
            item.Elements.Add(new SrgsItem(text));
            item.Elements.Add(new SrgsTag(string.Format(CultureInfo.InvariantCulture, "id={0}", id)));
            oneof.Add(item);
        }
        #endregion

        //*******************************************************************
        //
        // Private Members
        //
        //*******************************************************************

        #region Private Members
        internal DependencyObject _element;
        internal abstract void DoAction();
        internal delegate void DoActionDelegate();
        internal int _id;
        #endregion
    }
    #endregion

    #region Button
    // Button
    internal class ButtonSpeechObject : SpeechObject
    {
        internal ButtonSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((Button)element).Content;
            base.Initialize(element, text, oneOfList, id);
        }

        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(ClickButton);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void ClickButton()
        {
           Debug.Assert(_element is IInvokeProvider);
            ((IInvokeProvider)_element).Invoke();
        }
    }
    #endregion

    #region RadioButton
    internal class RadioButtonSpeechObject : SpeechObject
    {
        internal RadioButtonSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((RadioButton)element).Content;
            base.Initialize(element, text, oneOfList, id);
        }

        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(SelectRadioButton);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }
        internal void SelectRadioButton()
        {
            RadioButton rb = (RadioButton)_element;
            rb.IsChecked = true;
        }
    }
    #endregion

    #region ListBoxItem
    internal class ListBoxItemSpeechObject : SpeechObject
    {
        internal ListBoxItemSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((ListBoxItem)element).Content;
            base.Initialize(element, text, oneOfList, id);
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(SelectListBoxItem);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void SelectListBoxItem()
        {
            ListBoxItem lbi = (ListBoxItem)_element;
            lbi.IsSelected = !lbi.IsSelected;
        }
    }
    #endregion

    #region ComboBox
    internal class ComboBoxSpeechObject : SpeechObject
    {
        internal ComboBoxSpeechObject(DependencyObject element, string label, SrgsOneOf oneOfList, int id)
        {
            base.Initialize(element, label, oneOfList, id);
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(SelectComboBox);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void SelectComboBox()
        {
            ComboBox cb = (ComboBox)_element;
            cb.Focus();
            cb.IsDropDownOpen = !cb.IsDropDownOpen;
        }
    }
    #endregion

    #region ComboBoxItem
    internal class ComboBoxItemSpeechObject : SpeechObject
    {
        internal ComboBoxItemSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((ComboBoxItem)element).Content;
            base.Initialize(element, text, oneOfList, id);
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(SelectComboBoxItem);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void SelectComboBoxItem()
        {
            ComboBoxItem cbi = (ComboBoxItem)_element;
            cbi.IsSelected = !cbi.IsSelected;
            ((ComboBox)cbi.Parent).IsDropDownOpen = false;
        }
    }
    #endregion
    
    #region MenuItem
    internal class MenuItemSpeechObject : SpeechObject
    {
        internal MenuItemSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((MenuItem)element).Header;
            base.Initialize(element, text, oneOfList, id);
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(SelectMenuItem);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void SelectMenuItem()
        {
            ((IInvokeProvider)_element).Invoke();
        }
    }
    #endregion

    #region TextBox
    internal class TextBoxSpeechObject : SpeechObject
    {
        internal TextBoxSpeechObject(DependencyObject element, string label, SpeechRecognizer recognizer, SrgsOneOf oneOfList, int id)
        {
            base.Initialize(element, label, oneOfList, id);
            _recognizer = recognizer;
            TextBox tb = (TextBox)element;
            tb.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(TextBoxLostFocus);
            tb.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(TextBoxGotFocus);
            if (tb.IsFocused)
            {
                FocusTextBox();
            }
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(FocusTextBox);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void FocusTextBox()
        {
            TextBox tb = (TextBox)_element;
            tb.Focus();
            if (tb.InputScope != null)
            {
                if (tb.InputScope.Names != _currentInputScope)
                {
                    // input scope has changed
                    if (_grammar != null)
                    {
                        _recognizer.UnloadGrammar(_grammar);
                        _grammar = null;
                    }
                    _currentInputScope = tb.InputScope.Names;
                }
                if (_grammar == null)
                {
                    ResourceManager resourceManager = new ResourceManager("GrammarFiles", System.Reflection.Assembly.GetExecutingAssembly());
                    Stream stream = new MemoryStream((byte[])resourceManager.GetObject(@"GrammarLibrary"));

                    switch (tb.InputScope.Names)
                    {
                        case InputScopeName.Date:
                            _grammar = new Grammar(stream, "Date");
                            _grammar.SpeechRecognized += new EventHandler<RecognitionEventArgs>(DateRecognized);
                            break;
                        case InputScopeName.Number:
                        case InputScopeName.Digits:
                            _grammar = new Grammar(stream, "integer");
                            _grammar.SpeechRecognized += new EventHandler<RecognitionEventArgs>(DigitRecognized);
                            break;
                        default:
                            break;
                    }
                    if (_grammar != null)
                    {
                        _recognizer.LoadGrammar(_grammar);
                    }
                }
                if (_grammar != null)
                {
                    _grammar.Enabled = true;
                }
            }
            else
            {
                if (_grammar != null)
                {
                    // remove the grammar if it was previously set
                    _recognizer.UnloadGrammar(_grammar);
                    _grammar = null;
                }
            }
        }
        internal delegate void SpeechRecognizedHandler(RecognitionResult result);
        internal InputScopeName _currentInputScope;

        void TextBoxGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DoAction();
        }

        void TextBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_grammar != null)
            {
                _grammar.Enabled = false;
            }
        }

        void DateRecognized(object sender, RecognitionEventArgs e)
        {
            SpeechRecognizedHandler action = new SpeechRecognizedHandler(InsertDate);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action, e.Result);
        }

        void DigitRecognized(object sender, RecognitionEventArgs e)
        {
            SpeechRecognizedHandler action = new SpeechRecognizedHandler(InsertDigit);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action, e.Result);
        }

        void InsertDate(RecognitionResult result)
        {
            TextBox tb = (TextBox)_element;
            if (result.Semantics.Count != 0)
            {
                DateTime date = new DateTime(
                    Convert.ToInt32((string)result.Semantics["Year"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32 ((string) result.Semantics ["Month"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32 ((string) result.Semantics ["Day"].Value, CultureInfo.InvariantCulture));

                tb.Text = date.ToShortDateString();
            }
            else
            {
                tb.Text = ConvertTextToDate(result.Text);
            }
        }

        void InsertDigit(RecognitionResult result)
        {
            TextBox tb = (TextBox)_element;
            if (result.Semantics.Value != null)
            {
                tb.Text = (string)result.Semantics.Value;
            }
            else
            {
                tb.Text = ConvertTextToNumber(result.Text);
            }
        }

        private static string ConvertTextToNumber(string text)
        {
            return text;
        }

        private static string ConvertTextToDate (string text)
        {
            return text;
        }

        private Grammar _grammar;
        private SpeechRecognizer _recognizer;
    }
    #endregion

    #region CheckBox
    internal class CheckBoxSpeechObject : SpeechObject
    {
        internal CheckBoxSpeechObject(DependencyObject element, SrgsOneOf oneOfList, int id)
        {
            string text = (string)((CheckBox)element).Content;
            base.Initialize(element, text, oneOfList, id);
        }
        internal override void DoAction()
        {
            DoActionDelegate action = new DoActionDelegate(ToggleCheckBox);
            _element.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        internal void ToggleCheckBox()
        {
            CheckBox cb = (CheckBox)_element;
            cb.IsChecked = !cb.IsChecked;
        }
    }

    internal class CheckBoxSpeechObjectOn : CheckBoxSpeechObject
    {
        internal CheckBoxSpeechObjectOn(DependencyObject element, SrgsOneOf oneOfList, int id) : base(element, oneOfList, id) { }
    }

    internal class CheckBoxSpeechObjectOff : CheckBoxSpeechObject
    {
        internal CheckBoxSpeechObjectOff(DependencyObject element, SrgsOneOf oneOfList, int id) : base(element, oneOfList, id) { }
    }
    #endregion
}
