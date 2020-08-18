using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Handles manipulating the perspective on the DrawingArea.
    /// </summary>
    public class Camera
    {
        private Vector2 position;
        private Matrix zoomMatrix;

        #region Properties

        public Vector2 Position { get => position; set => position = value; }

        public float Zoom { get; private set; }

        public Matrix ZoomMatrix { get => zoomMatrix; }

        /// <summary>
        /// Returns the currentMousePosition corrected for zoom and position of the camera.
        /// </summary>
        public Vector2 CurrentMousePosition
        {
            get 
            {
                return (InputManager.CurrentMousePosition() - position) / Zoom;
            }
        }

        /// <summary>
        /// Returns the previousMousePosition corrected for zoom and position of the camera.
        /// </summary>
        public Vector2 PreviousMousePosition
        {
            get
            {
                return (InputManager.PreviousMousePosition() - position) / Zoom;
            }
        }

        #endregion

        public Camera()
        {
            Zoom = 1.0f;
            zoomMatrix = new Matrix
                         (
                            new Vector4(Zoom, 0, 0, 0),
                            new Vector4(0, Zoom, 0, 0),
                            new Vector4(0, 0, 1, 0),
                            new Vector4(/*point.X - */position.X, /*point.Y - */position.Y, 0, 1)
                         );
        }

        public void Update()
        {
            UpdateCameraDragging();
            UpdateCameraZooming();
        }

        private void UpdateCameraDragging()
        {
            if (InputManager.IsMiddleMouseButtonDown())
            {
                Vector2 positionTemp = position;
                position += (InputManager.CurrentMousePosition() - InputManager.PreviousMousePosition());
                if (position.X > 0) position.X = positionTemp.X;
                if (position.Y > 0) position.Y = positionTemp.Y;

                zoomMatrix.M41 = position.X;
                zoomMatrix.M42 = position.Y;
            }
        }   

        private void UpdateCameraZooming()
        {
            float currentScrollWheel = InputManager.CurrentScrollWheel();
            float previousScrollWheel = InputManager.PreviousScrollWheel();

            // ScrollWheelMoved ?
            // Could also call InputManager.scrollWheelMoved, but we need current- and previousScrollWheel
            // anyway. So we can just test that here without having the extra method call.
            if (currentScrollWheel != previousScrollWheel)
            {
                if (currentScrollWheel < previousScrollWheel && Zoom > 0.027f)
                {
                    Zoom -= 0.01f + (0.04f * Zoom);
                }
                else if (currentScrollWheel > previousScrollWheel)
                {
                    Zoom += 0.01f + (0.1f * Zoom);
                }

                zoomMatrix.M11 = Zoom;
                zoomMatrix.M22 = Zoom;
            }            
        }
    }
}
