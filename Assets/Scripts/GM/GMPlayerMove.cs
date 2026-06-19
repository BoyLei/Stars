using UnityEngine;

public class GMPlayerMove : MonoBehaviour
{
    public Transform Model;
    public float speed = 3;

    private CharacterController characterController;

    private void Start()
    {
        characterController = this.GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1)
        {
            Vector3 targetDir = new Vector3(h, 0, v);
            characterController.SimpleMove(targetDir * speed);
            if (Model != null)
            {
                Model.LookAt(targetDir + transform.position);
            }
            //characterController.SimpleMove(transform.forward * speed);
        }
    }
}

