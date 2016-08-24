using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public static class TransitionHelper
    {
        public static void Show(UIElement target, NodeTransition transitionType, Action callback)
        {
            TransitionElement transitionFactory = ResolveForwardWaveTransition(transitionType);

            if (transitionFactory != null)
            {
                ITransition transition = transitionFactory.GetTransition(target);

                transition.Completed += delegate
                {
                    transition.Stop();
                    Core.UI.IgnoreBackButton = false;

                    callback.Invoke();
                };

                Core.UI.IgnoreBackButton = true;
                transition.Begin();
            }
            else
                callback.Invoke();
        }

        public static void Hide(UIElement target, NodeTransition transitionType, Action callback)
        {
            TransitionElement transitionFactory = ResolveReverseWaveTransition(transitionType);

            if (transitionFactory != null)
            {
                ITransition transition = transitionFactory.GetTransition(target);

                transition.Completed += delegate
                {
                    transition.Stop();
                    Core.UI.IgnoreBackButton = false;

                    callback.Invoke();
                };

                Core.UI.IgnoreBackButton = true;
                transition.Begin();
            }
            else
                callback.Invoke();
        }

        private static TransitionElement ResolveForwardWaveTransition(NodeTransition transitionType)
        {
            TransitionElement res = null;

            switch (transitionType)
            {
                case NodeTransition.SlideFromRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideFromLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SpringOn:
                    {
                        SwivelTransition swt = new SwivelTransition();
                        swt.Mode = SwivelTransitionMode.ForwardIn;

                        res = swt;
                        break;
                    }

                case NodeTransition.SpringOff:
                    {
                        SwivelTransition swt = new SwivelTransition();
                        swt.Mode = SwivelTransitionMode.BackwardOut;

                        res = swt;
                        break;
                    }

                case NodeTransition.SlideFromTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideFromBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeOut;

                        res = slt;
                        break;
                    }
            }

            return res;
        }

        private static TransitionElement ResolveReverseWaveTransition(NodeTransition transitionType)
        {
            TransitionElement res = null;

            switch (transitionType)
            {
                case NodeTransition.SlideFromRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideFromLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToRight:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SpringOn:
                    {
                        SwivelTransition swt = new SwivelTransition();
                        swt.Mode = SwivelTransitionMode.BackwardOut;

                        res = swt;
                        break;
                    }

                case NodeTransition.SpringOff:
                    {
                        SwivelTransition swt = new SwivelTransition();
                        swt.Mode = SwivelTransitionMode.ForwardIn;

                        res = swt;
                        break;
                    }

                case NodeTransition.SlideFromTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideFromBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideLeftFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOverFromBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeOut;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToLeft:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideRightFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToTop:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideDownFadeIn;

                        res = slt;
                        break;
                    }

                case NodeTransition.SlideOffToBottom:
                    {
                        SlideTransition slt = new SlideTransition();
                        slt.Mode = SlideTransitionMode.SlideUpFadeIn;

                        res = slt;
                        break;
                    }
            }

            return res;
        }
    }
}
