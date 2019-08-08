using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class UiObjectContainer : IUiObject, IResizeResponsive
    {
        private UiObjectHorizontalPositioning _HorizontalPositioning;
        private Margin _Margins;
        private Vector2u _OriginalParentSize;
        private Vector2f _Position;
        private Vector2u _Size;
        private UiObjectVerticalPositioning _VerticalPositioning;

        public UiObjectContainer()
        {
            UiObjects = new ObservableCollection<IUiObject>();
            UiObjects.CollectionChanged += OnCollectionChanged;
            Margins = new Margin();
        }

        public Vector2u ParentSize { get; private set; }

        public UiObjectVerticalPositioning VerticalPositioning
        {
            get => _VerticalPositioning;
            set
            {
                _VerticalPositioning = value;
                FullUiObjectsPositioningUpdate();
            }
        }

        public UiObjectHorizontalPositioning HorizontalPositioning
        {
            get => _HorizontalPositioning;
            set
            {
                _HorizontalPositioning = value;
                FullUiObjectsPositioningUpdate();
            }
        }

        public bool VerticalAutoStacking { get; set; }
        public bool HorizontalAutoStacking { get; set; }
        public bool JustifyX { get; set; }
        public bool JustifyY { get; set; }

        public ObservableCollection<IUiObject> UiObjects { get; }

        public Vector2u OriginalParentSize
        {
            get => _OriginalParentSize;
            set
            {
                _OriginalParentSize = value;
                Size = _OriginalParentSize;
            }
        }

        public Vector2u Size
        {
            get => _Size;
            set
            {
                value.X += Margins.Left + Margins.Right;
                value.Y += Margins.Top + Margins.Bottom;

                _Size = value;
                OnSizeChanged(this, new SizeEventArgs(new SizeEvent {Height = _Size.X, Width = _Size.Y}));
            }
        }

        public Vector2f Position
        {
            get => _Position;
            set
            {
                _Position = value;
                FullUiObjectsPositioningUpdate();
            }
        }

        public Vector2f Origin { get; set; }

        public Margin Margins
        {
            get => _Margins;
            set
            {
                _Margins = value;

                Size = _Size;
            }
        }


        public IEnumerable<IUiObject> SubscribableObjects()
        {
            return UiObjects;
        }

        private void FullUiObjectsPositioningUpdate()
        {
            float _autoStackMiddleSpacingX = Size.X / (UiObjects.Count + 1f);
            float _autoStackMiddleSpacingY = Size.Y / (UiObjects.Count + 1f);
            float _totalObjectsSizeX = UiObjects.Select(uiObject => (float) uiObject.Size.X).Sum();
            float _totalObjectsSizeY = UiObjects.Select(uiObject => (float) uiObject.Size.Y).Sum();
            float _cumulativeWidth = 0f;
            float _cumulativeHeight = 0f;

            for (int _i = 0; _i < UiObjects.Count; _i++)
            {
                float _xPos = 0f;
                float _yPos = 0f;

                switch (HorizontalPositioning)
                {
                    case UiObjectHorizontalPositioning.Left:
                        _xPos = HorizontalAutoStacking
                            ? _cumulativeWidth + (UiObjects[_i].Size.X / 2f)
                            : UiObjects[_i].Size.X / 2f;
                        break;
                    case UiObjectHorizontalPositioning.Middle:
                        _xPos = HorizontalAutoStacking
                            ? ((Size.X / 2f) - (_totalObjectsSizeX / 2f)) + _cumulativeWidth + (UiObjects[_i].Size.X / 2f)
                            : Size.X / 2f;
                        break;
                    case UiObjectHorizontalPositioning.Right:
                        _xPos = HorizontalAutoStacking
                            ? Size.X - (_cumulativeWidth + (UiObjects[_i].Size.X / 2f))
                            : Size.X - (UiObjects[_i].Size.X / 2f);
                        break;
                    case UiObjectHorizontalPositioning.Justify:
                        _xPos = (_autoStackMiddleSpacingX * (_i + 1)) - (UiObjects[_i].Size.X / 2f);
                        break;
                    case UiObjectHorizontalPositioning.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (VerticalPositioning)
                {
                    case UiObjectVerticalPositioning.Top:
                        _yPos = VerticalAutoStacking
                            ? _cumulativeHeight + (UiObjects[_i].Size.Y / 2f)
                            : UiObjects[_i].Size.Y / 2f;
                        break;
                    case UiObjectVerticalPositioning.Middle:
                        _yPos = VerticalAutoStacking
                            ? ((Size.Y / 2f) - (_totalObjectsSizeY / 2f)) + _cumulativeHeight + (UiObjects[_i].Size.Y / 2f)
                            : Size.Y / 2f;
                        break;
                    case UiObjectVerticalPositioning.Bottom:
                        _yPos = VerticalAutoStacking
                            ? Size.Y - (_cumulativeHeight + (UiObjects[_i].Size.Y / 2f))
                            : Size.Y - (UiObjects[_i].Size.Y / 2f);
                        break;
                    case UiObjectVerticalPositioning.Justify:
                        _yPos = (_autoStackMiddleSpacingY * (_i + 1)) - (UiObjects[_i].Size.Y / 2f);
                        break;
                    case UiObjectVerticalPositioning.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _cumulativeWidth += UiObjects[_i].Size.X;
                _cumulativeHeight += UiObjects[_i].Size.Y;

                UiObjects[_i].Position = new Vector2f(_xPos + Position.X, _yPos + Position.Y);
            }
        }


        #region EVENTS

        public event EventHandler<SizeEventArgs> Resized;

        private void OnSizeChanged(object sender, SizeEventArgs args)
        {
            FullUiObjectsPositioningUpdate();

            Resized?.Invoke(sender, args);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            FullUiObjectsPositioningUpdate();
        }

        public void OnParentResized(object sender, SizeEventArgs args)
        {
            ParentSize = new Vector2u(args.Width, args.Height);
        }

        #endregion
    }
}