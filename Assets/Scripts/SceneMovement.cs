using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneMovement : MonoBehaviour
{      
    Animator anim;

    public bool isCollidingWithRespawn = false;

    void Start()
    {
        anim = GetComponent<Animator>(); // Initialize the animator component
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();

        if (isCollidingWithRespawn){
            SceneManager.LoadScene(1);
        }
    }
    

}

