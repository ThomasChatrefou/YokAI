using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace YokAI.POC
{
    public class PieceMonobehaviour : MonoBehaviour
    { 
        [SerializeField] private int pieceSize = 1;

        private SpriteRenderer indicator;
        private Camera _camera;
        private bool _isGrabbed = false;
        private Vector3 originalPosition;
        private Coroutine followMouseCoroutine;

        private int pieceID;

        public int PieceID
        {
            get => pieceID;
            set => pieceID = value;
        }

        public Vector3 OriginalPosition
        {
            get => originalPosition;
            set => originalPosition = value;
        }

        private void Start()
        {
            _camera = Camera.main;
            indicator = GameObject.FindWithTag("Indicator").GetComponent<SpriteRenderer>();
            
            originalPosition = transform.position;
        }

        private void OnMouseDown()
        { 
            _isGrabbed = true;
            FlipFlopIndicator();
            
            BanManager.Instance.MoveIndicator(pieceID, true);
            followMouseCoroutine = StartCoroutine(FollowMouse());
        }

        private void OnMouseUp()
        {
            if (_isGrabbed)
            {
                Vector2 targetPos = GetNearestPos(transform.position);
                if (IsInBounds() && BanManager.Instance.CheckIfCanMake(pieceID, originalPosition, targetPos)) //check if move is legal and/or make move
                    originalPosition = targetPos;
                
                StopCoroutine(followMouseCoroutine);
                _isGrabbed = false;
                transform.position = originalPosition;
                FlipFlopIndicator();
                BanManager.Instance.MoveIndicator(pieceID, false);
            }
        }

        private IEnumerator FollowMouse()
        {
            while (_isGrabbed)
            {
                Vector2 pos = _camera.ScreenToWorldPoint(Input.mousePosition);
                Transform goTransform = transform;
                
                goTransform.position = pos;
                if (IsInBounds())
                {
                    indicator.transform.position = GetNearestPos(goTransform.position);
                }
                else
                {
                    indicator.transform.position = originalPosition;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private bool IsInBounds()
        {
            Rect bounds = new Rect(-0.5f, -0.5f, 3, 4);

            return bounds.Contains(transform.position);
        }

        private Vector2 GetNearestPos(Vector3 worldPosition)
        {
            Vector2Int gridCoord = new(
                Mathf.RoundToInt(worldPosition.x / pieceSize),
                Mathf.RoundToInt(worldPosition.y / pieceSize)
                );

            return gridCoord;
        }

        private void FlipFlopIndicator()
        {
            Color indicatorColor = BanManager.Instance.IndicatorColor;
            float alpha = indicator.color.a > 0 ? 0 : indicatorColor.a;
            
            indicator.color = new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, alpha);
        }
    }
}