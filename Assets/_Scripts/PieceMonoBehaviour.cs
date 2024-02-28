using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace YokAI
{
    public class PieceMonobehaviour : MonoBehaviour
    {
        [SerializeField] private int pieceSize = 1;
        [SerializeField] private bool IsPooled = false;
        [SerializeField, ShowIf("IsPooled")] private PieceMonobehaviour pieceToSpawn;
        [SerializeField] private Color indicatorColor;
        
        private SpriteRenderer indicator;
        private Camera _camera;
        private bool _isGrabbed = false;
        private Vector3 originalPosition;
        private Coroutine followMouseCoroutine;
        private bool _isSpawned;

        private PieceMonobehaviour pieceSpawned;

        public Vector3 OriginalPosition
        {
            get => originalPosition;
            set => originalPosition = value;
        }

        public bool IsSpawned
        {
            get => _isSpawned;
            set => _isSpawned = value;
        }

        private void Start()
        {
            _camera = Camera.main;
            indicator = GameObject.FindWithTag("Indicator").GetComponent<SpriteRenderer>();
            
            originalPosition = transform.position;

            if (_isSpawned)
                OnMouseDown();
        }

        private void OnMouseDown()
        { 
            //TODO: Link placement with actual board, and link pieces coming from pool with actual pool :D
            
            _isGrabbed = true;
            
            if (IsPooled)
            {
                pieceSpawned = Instantiate(pieceToSpawn);
                pieceSpawned.gameObject.GetComponent<Collider2D>().enabled = false;
                pieceSpawned.IsSpawned = true;
                return;
            }
            
            FlipFlopIndicator();
             
            followMouseCoroutine = StartCoroutine(FollowMouse());
        }

        private void OnMouseUp()
        {
            if (_isGrabbed)
            {
                if (IsPooled)
                { 
                    pieceSpawned.OnMouseUp();
                    pieceSpawned.gameObject.GetComponent<Collider2D>().enabled = true;
                    return;
                }
                if (IsInBounds())
                    originalPosition = GetNearestPos(transform.position);
                
                StopCoroutine(followMouseCoroutine);
                _isGrabbed = false;
                transform.position = originalPosition;
                FlipFlopIndicator();
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
            float alpha = indicator.color.a > 0 ? 0 : indicatorColor.a;
            
            indicator.color = new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, alpha);
        }
    }
}
