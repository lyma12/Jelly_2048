using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.JellyMerge
{
    public class Swipe : MonoBehaviour
    {
        private int magnitude = 50;

        private bool tap, swipeLeft, swipeRight, swipeTop, swipeBottom;
        private bool isDragging = false;
        private Vector2 startTouch, swipeDelta;

        public Vector2 SwipeDelta { get { return swipeDelta; } }
        public bool SwipeLeft { get { return swipeLeft; } }
        public bool SwipeRight { get { return swipeRight; } }
        public bool SwipeTop { get { return swipeTop; } }
        public bool SwipeBottom { get { return swipeBottom; } }

        private void Update()
        {
            tap = swipeLeft = swipeRight = swipeTop = swipeBottom = false;

            #region Standalone
            if (InputController.ClickAction.WasPressedThisFrame())
            {
                tap = true;
                isDragging = true;

                startTouch = InputController.MousePosition;
            }
            else if (InputController.ClickAction.WasReleasedThisFrame())
            {
                Reset();
            }
            #endregion

            if (isDragging)
            {
                swipeDelta = InputController.MousePosition - startTouch;

                if (swipeDelta.magnitude > magnitude)
                {
                    float x = swipeDelta.x;
                    float y = swipeDelta.y;

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x > 0)
                        {
                            swipeRight = true;
                        }
                        else
                        {
                            swipeLeft = true;
                        }
                    }
                    else
                    {
                        if (y > 0)
                        {
                            swipeTop = true;
                        }
                        else
                        {
                            swipeBottom = true;
                        }
                    }

                    Reset();
                }
            }
        }

        private void Reset()
        {
            startTouch = swipeDelta = Vector2.zero;

            isDragging = false;
        }
    }
}