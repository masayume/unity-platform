using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepeController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    EnemyController enemyController;

    bool isFacingRight;

    bool isFollowingPath;
    Vector3 pathStartPoint;
    Vector3 pathEndPoint;
    Vector3 pathMidPoint;
    float pathTimeStart;
    public float bezierTime = 1f;
    public float bezierDistance = 1f;
    public Vector3 bezierHeight = new Vector3(0, 0.8f, 0);

    public enum MoveDirections { Left, Right };

    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;

    // Start is called before the first frame update
    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        rb2d = enemyController.GetComponent<Rigidbody2D>();

        // which way to face ? (spritesheet is right but game is left)

        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            pathTimeStart += Time.deltaTime; // compensation for time passing and path calculation while frozen
            return;
        }

        animator.Play("Pepe_Flying");

        if (!isFollowingPath)
        {
            float distance = (isFacingRight) ? bezierDistance : -bezierDistance;
            pathStartPoint = rb2d.transform.position;
            pathEndPoint = new Vector3(pathStartPoint.x + distance, pathStartPoint.y, pathStartPoint.z);
            pathEndPoint = pathStartPoint + (((pathEndPoint - pathStartPoint) / 2 ) + bezierHeight);
            pathTimeStart = Time.time;
            isFollowingPath = true;
        }
        else 
        {
            float percentage = (Time.time - pathTimeStart) / bezierTime; // 0 - 1, analog to LERP
            rb2d.transform.position = UtilityFunctions.CalculateQuadraticBezierPoint(
                pathStartPoint, pathMidPoint, pathEndPoint, percentage);
            if (percentage >= 1f)
            {
                bezierHeight *= -1; // second half of the y curve
                isFollowingPath = false;
            }
        }
    }

    public void SetMoveDirection(MoveDirections direction)
    {
        moveDirection = direction;
        if (moveDirection == MoveDirections.Left)
        {
            if (isFacingRight)
            {
                isFacingRight = !isFacingRight;
                enemyController.Flip();
            }
        }
        else 
        {
            if (!isFacingRight)
            {
                isFacingRight = !isFacingRight;
                enemyController.Flip();
            }
        }
    }

    public void ResetFollowingPath()
    {
        // if position is manually changed while following curve 
        // then the Y position decays until the path finishes and 
        // new control points are calculated
        // call this function to force calculation of new points
        isFollowingPath = false; 

    }



}
