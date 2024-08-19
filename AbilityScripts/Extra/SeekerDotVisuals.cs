using GameNetcodeStuff;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts.Extra
{
    public class SeekerDotVisuals : MonoBehaviour
    {
        public PlayerControllerB targetedPlayer;
        public Vector3 targetPosition;
        float dotVisualSize = 0.05f; // Units
        SpriteRenderer spriteRender;
        void Start ()
        {
            Sprite dotSprite = AbilitySpriteManager.GetSprite("SoftDot");
            spriteRender = gameObject.AddComponent<SpriteRenderer>();
            spriteRender.sprite = dotSprite;
            transform.localScale = Vector3.one * dotVisualSize;
        }

        void Update()
        {
            if (GameNetworkManager.Instance.localPlayerController)
            {
                Transform localCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;

                gameObject.transform.LookAt(localCamera.position);
                if (Plugin.seekerPlayer)
                {
                    if ((localCamera.position - targetPosition).magnitude < 3)
                    {
                        spriteRender.enabled = false;
                        return;
                    }
                    else
                    {
                        spriteRender.enabled = true;
                    }

                    float distanceFromSeeker = (Plugin.seekerPlayer.transform.position - targetPosition).magnitude;
                    Vector3 trackDirection = (targetPosition - gameObject.transform.position).normalized;

                    float tValue = Mathf.Clamp((distanceFromSeeker-9)/30, 0, 2);


                    if (tValue < 1)
                    {
                        // Close = Red
                        spriteRender.color = new(tValue, 1, 0);
                    }
                    else
                    {
                        // Far = Green
                        spriteRender.color = new(1, 2-tValue, 0);
                    }

                    gameObject.transform.position = localCamera.position + trackDirection;
                }
            }
        }
    }
}
