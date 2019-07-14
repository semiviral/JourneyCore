using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class UIObjectContainer : IUIObject, IResizeResponsive
    {
        private UIObjectHorizontalPositioning _HorizontalPositioning;
        private Vector2u _OriginalParentSize;
        private Vector2f _Position;
        private Vector2u _Size;
        private UIObjectVerticalPositioning _VerticalPositioning;
        public Vector2u ParentSize { get; private set; }

        public UIObjectVerticalPositioning VerticalPositioning
        {
            get => _VerticalPositioning;
            set
            {
                _VerticalPositioning = value;
                FullUIObjectsPositioningUpdate();
            }
        }

        public UIObjectHorizontalPositioning HorizontalPositioning
        {
            get => _HorizontalPositioning;
            set
            {
                _HorizontalPositioning = value;
                FullUIObjectsPositioningUpdate();
            }
        }

        public bool VerticalAutoStacking { get; set; }
        public bool HorizontalAutoStacking { get; set; }
        public bool JustifyX { get; set; }
        public bool JustifyY { get; set; }

        public ObservableCollection<IUIObject> UIObjects { get; }

        public UIObjectContainer()
        {
            UIObjects = new ObservableCollection<IUIObject>();
            UIObjects.CollectionChanged += OnCollectionChanged;
        }

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
                _Size = value;
                OnSizeChanged(this, new SizeEventArgs(new SizeEvent { Height = _Size.X, Width = _Size.Y }));
            }
        }

        public Vector2f Position
        {
            get => _Position;
            set
            {
                _Position = value;
                FullUIObjectsPositioningUpdate();
            }
        }

        public Vector2f Origin { get; set; }


        public IEnumerable<IUIObject> SubscribableObjects()
        {
            return UIObjects;
        }
        
        private void FullUIObjectsPositioningUpdate()
        {
            float autoStackMiddleSpacingX = Size.X / (UIObjects.Count + 1f);
            float autoStackMiddleSpacingY = Size.Y / (UIObjects.Count + 1f);
            float totalObjectsSizeX = UIObjects.Select(uiObject => (float)uiObject.Size.X).Sum();
            float totalObjectsSizeY = UIObjects.Select(uiObject => (float)uiObject.Size.Y).Sum();
            float cumulativeWidth = 0f;
            float cumulativeHeight = 0f;

            for (int i = 0; i < UIObjects.Count; i++)
            {
                float xPos = 0f;
                float yPos = 0f;

                switch (HorizontalPositioning)
                {
                    case UIObjectHorizontalPositioning.Left:
                        xPos = HorizontalAutoStacking
                            ? cumulativeWidth + UIObjects[i].Size.X / 2f
                            : UIObjects[i].Size.X / 2f;
                        break;
                    case UIObjectHorizontalPositioning.Middle:
                        xPos = HorizontalAutoStacking ? Size.X / 2f - totalObjectsSizeX / 2f + cumulativeWidth + UIObjects[i].Size.X / 2f : Size.X / 2f;
                        break;
                    case UIObjectHorizontalPositioning.Right:
                        xPos = HorizontalAutoStacking
                            ? Size.X - (cumulativeWidth + UIObjects[i].Size.X / 2f)
                            : Size.X - UIObjects[i].Size.X / 2f;
                        break;
                    case UIObjectHorizontalPositioning.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (VerticalPositioning)
                {
                    case UIObjectVerticalPositioning.Top:
                        yPos = VerticalAutoStacking
                            ? cumulativeHeight + UIObjects[i].Size.Y / 2f
                            : UIObjects[i].Size.Y / 2f;
                        break;
                    case UIObjectVerticalPositioning.Middle:
                        yPos = VerticalAutoStacking ? Size.Y / 2f - totalObjectsSizeY / 2f + cumulativeHeight + UIObjects[i].Size.Y / 2f : Size.Y / 2f;
                        break;
                    case UIObjectVerticalPositioning.Bottom:
                        yPos = VerticalAutoStacking
                            ? Size.Y - (cumulativeHeight + UIObjects[i].Size.Y / 2f)
                            : Size.Y - UIObjects[i].Size.Y / 2f;
                        break;
                    case UIObjectVerticalPositioning.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                cumulativeWidth += UIObjects[i].Size.X;
                cumulativeHeight += UIObjects[i].Size.Y;
                
                UIObjects[i].Position = new Vector2f(xPos + Position.X, yPos + Position.Y);
            }
        }


        #region EVENTS

        public event EventHandler<SizeEventArgs> Resized;

        private void OnSizeChanged(object sender, SizeEventArgs args)
        {
            FullUIObjectsPositioningUpdate();

            Resized?.Invoke(sender, args);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            FullUIObjectsPositioningUpdate();
        }

        public void OnParentResized(object sender, SizeEventArgs args)
        {
            ParentSize = new Vector2u(args.Width, args.Height);
        }

        #endregion
    }
}