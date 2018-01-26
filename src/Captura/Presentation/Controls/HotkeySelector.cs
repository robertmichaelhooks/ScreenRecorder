﻿using Captura.Models;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Captura
{
    public class HotkeySelector : Button
    {
        bool _editing;

        Hotkey _hotkey;
        
        public static readonly DependencyProperty HotkeyModelProperty = DependencyProperty.Register(nameof(HotkeyModel), typeof(Hotkey), typeof(HotkeySelector), new UIPropertyMetadata(PropertyChangedCallback));

        static void PropertyChangedCallback(DependencyObject Sender, DependencyPropertyChangedEventArgs Args)
        {
            if (Sender is HotkeySelector selector && Args.NewValue is Hotkey hotkey)
            {
                selector._hotkey = hotkey;

                selector.TextColor();

                hotkey.PropertyChanged += (S, E) =>
                {
                    if (E.PropertyName == nameof(Hotkey.IsActive))
                        selector.TextColor();
                };

                selector.Content = hotkey.ToString();
            }
        }

        public Hotkey HotkeyModel
        {
            get => (Hotkey) GetValue(HotkeyModelProperty);
            set => SetValue(HotkeyModelProperty, value);
        }

        void HotkeyEdited(Key NewKey, Modifiers NewModifiers)
        {
            HotkeyEdited((Keys) KeyInterop.VirtualKeyFromKey(NewKey), NewModifiers);
        }

        void TextColor()
        {
            if (_hotkey.IsActive && !_hotkey.IsRegistered)
            {
                if (ColorConverter.ConvertFromString("#ef5350") is Color c)
                    Background = new SolidColorBrush(c);

                Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                ClearValue(BackgroundProperty);

                ClearValue(ForegroundProperty);
            }
        }

        void HotkeyEdited(Keys NewKey, Modifiers NewModifiers)
        {
            _hotkey.Change(NewKey, NewModifiers);

            // Red Text on Error
            TextColor();

            Content = _hotkey.ToString();

            _editing = false;
        }
        
        protected override void OnClick()
        {
            base.OnClick();

            _editing = !_editing;

            Content = _editing ? "Press new Hotkey..." : _hotkey.ToString();
        }

        protected override void OnLostFocus(RoutedEventArgs E)
        {
            base.OnLostFocus(E);

            CancelEditing();
        }

        void CancelEditing()
        {
            if (!_editing)
                return;

            _editing = false;
            Content = _hotkey.ToString();
        }

        static bool IsValid(KeyEventArgs e)
        {
            return e.Key != Key.None // Some key must pe pressed
                && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Windows) // Windows Key is reserved by OS
                && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl // Modifier Keys alone are not supported
                && e.Key != Key.LeftAlt && e.Key != Key.RightAlt
                && e.Key != Key.LeftShift && e.Key != Key.RightShift;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs E)
        {
            // Ignore Repeats
            if (E.IsRepeat)
            {
                E.Handled = true;
                return;
            }

            if (_editing)
            {
                // Suppress event propagation
                E.Handled = true;

                switch (E.Key)
                {
                    case Key.Escape:
                        CancelEditing();
                        break;

                    case Key.System:
                        if (E.SystemKey == Key.LeftAlt || E.SystemKey == Key.RightAlt)
                            Content = "Alt + ...";
                        else HotkeyEdited(E.SystemKey, Modifiers.Alt);
                        break;

                    default:
                        if (IsValid(E))
                            HotkeyEdited(E.Key, (Modifiers)E.KeyboardDevice.Modifiers);

                        else
                        {
                            var modifiers = E.KeyboardDevice.Modifiers;

                            Content = "";

                            if (modifiers.HasFlag(ModifierKeys.Control))
                                Content += "Ctrl + ";

                            if (modifiers.HasFlag(ModifierKeys.Alt))
                                Content += "Alt + ";

                            if (modifiers.HasFlag(ModifierKeys.Shift))
                                Content += "Shift + ";

                            Content += "...";
                        }
                        break;
                }
            }

            base.OnPreviewKeyDown(E);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs E)
        {
            // Ignore Repeats
            if (E.IsRepeat)
                return;

            if (_editing)
            {
                // Suppress event propagation
                E.Handled = true;

                // PrintScreen is not recognized in KeyDown
                switch (E.Key)
                {
                    case Key.Snapshot:
                        HotkeyEdited(Keys.PrintScreen, (Modifiers)E.KeyboardDevice.Modifiers);
                        break;

                    case Key.System when E.SystemKey == Key.Snapshot:
                        HotkeyEdited(Keys.PrintScreen, Modifiers.Alt);
                        break;
                }
            }

            base.OnPreviewKeyUp(E);
        }
    }
}