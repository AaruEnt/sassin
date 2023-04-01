using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BF_FxMouse : MonoBehaviour
{
    private Camera mainCam;
    public ParticleSystem ps;
    public ParticleSystem psRightClick;
    public ParticleSystem psMiddleClick;
    private float raycastSize = 200f;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = this.GetComponent<Camera>();
    }
    void OnEnable()
    {
        mainCam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit, raycastSize))
            {
                if (!ps.isEmitting)
                {
                    ps.Play();
                }
                ps.transform.position = hit.point;
            }
        }
        else
        {
            ps.Stop();
        }

        if (Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit, raycastSize))
            {
                if (!psRightClick.isEmitting)
                {
                    psRightClick.Play();
                }
                psRightClick.transform.position = hit.point;
            }
        }
        else
        {
            psRightClick.Stop();
        }

        if (Input.GetMouseButton(2))
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit, raycastSize))
            {
                if (!psMiddleClick.isEmitting)
                {
                    psMiddleClick.Play();
                }
                psMiddleClick.transform.position = hit.point;
            }
        }
        else
        {
            psMiddleClick.Stop();
        }
#else
        if (Mouse.current.leftButton.isPressed)
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit, raycastSize))
            {
                if (!ps.isEmitting)
                {
                    ps.Play();
                }
                ps.transform.position = hit.point;
            }
            else
            {
                // ps.Stop();
            }
        }
        else
        {
            ps.Stop();
        }

        if (Mouse.current.rightButton.isPressed)
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit, raycastSize))
            {
                if (!psRightClick.isEmitting)
                {
                    psRightClick.Play();
                }
                psRightClick.transform.position = hit.point;
            }
            else
            {
                // ps.Stop();
            }
        }
        else
        {
            psRightClick.Stop();
        }
#endif
    }
}
