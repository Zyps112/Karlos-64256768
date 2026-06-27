using UnityEngine;

public class Animations_PlaceHolder : MonoBehaviour
{
    public Animator playerAnim;

    public bool relaoding;

    public bool drawing;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            relaoding = true;
        }
        else
        {
            relaoding = false;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            drawing = true;
        }
        else
        {
            drawing = false;
        }

        ReloadAnim();
        DrawAnim();
    }

    public void FireAnim(bool fire)
    {
        playerAnim.SetBool("Fire", fire);
    }

    public void ReloadAnim()
    {
        playerAnim.SetBool("Reload", relaoding);
    }

    public void DrawAnim()
    {
        playerAnim.SetBool("Draw", drawing);
    }
}
