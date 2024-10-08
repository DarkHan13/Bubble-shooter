using System;
using System.Collections;
using Ball.State;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MainMenu
{
    public class PreviewField : MonoBehaviour
    {
        public GameField Field;
        [SerializeField] private BallStateMachine ballPrefab;
        [SerializeField] private float offset = 0.1f;
        [SerializeField, Range(0.1f, 1f)] private float radius = 0.5f;

        private IEnumerator Start()
        {
            Application.targetFrameRate = 60;
            Camera cam = Camera.main;
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Field = new GameField(topRight.x, topRight.y, bottomLeft.y, radius, offset);
            var positions = Field.GenerateField();
            foreach (var position in positions)
            {
                var ball = Instantiate(ballPrefab, topRight, Quaternion.identity);
                ball.transform.localScale = Vector3.one * (2 * radius);
                ball.transform.SetParent(transform);
                ball.Init();

                ball.Context.TargetPosition = position;
                ball.Context.NextState = BallStateEnum.Static;
                ball.SetType((BallType)Random.Range(1, 6));
                ball.SetState(BallStateEnum.Moving);
                ball.OnMouseDownEvent += BallOnClick;
                yield return new WaitForSeconds(0.02f);
            }
        }

        private void BallOnClick(BallStateMachine ball)
        {
            ball.SetState(BallStateEnum.Popped);
            Field.UpdateStaticBalls();
            Field.TriggerFalling();
        }
    }
}
