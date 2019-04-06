using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubiksCube : MonoBehaviour
{
    public class Move
    {
        public Vector3 SignedAxis { get; private set; }
        public int LayerIndex { get; private set; }

        public Move(Vector3 signedAxis, int layerIndex)
        {
            SignedAxis = signedAxis;
            LayerIndex = layerIndex;
        }

        public void FlipDirection()
        {
            SignedAxis *= -1;
        }
    }

    [SerializeField]
    private int size = 3;
    [SerializeField]
    private GameObject cubiePrefab;
    [SerializeField]
    private float turnDuration = 0.5f;
    [SerializeField]
    private int shuffleSteps = 100;

    private Coroutine moving;
    private Coroutine undoing;
    private Coroutine shuffling;
    private List<Move> moves;
    public int GeneratedCubeSize { get; private set; }

    private void OnValidate()
    {
        size = Mathf.Max(2, size);
        turnDuration = Mathf.Max(0f, turnDuration);
    }

    public void GenerateCube()
    {
        if (cubiePrefab == null) { return; }

        ClearCube();

        Vector3 scale = Vector3.one / size;
        Vector3 offset = Vector3.one * ((1f / size * 0.5f) - 0.5f);
        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if ((z > 0 && z < size - 1) && (y > 0 && y < size - 1) && (x > 0 && x < size - 1))
                    {
                        continue;
                    }

                    Vector3 position = new Vector3(x, y, z) / size;
                    CreateCubie(position + offset, scale);
                }
            }
        }

        GeneratedCubeSize = size;
        moves = new List<Move>();
    }

    public void ClearCube()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    private void CreateCubie(Vector3 position, Vector3 scale)
    {
        Transform cubie = Instantiate(cubiePrefab).transform;
        cubie.name = cubiePrefab.name;
        cubie.parent = transform;
        cubie.localPosition = position;
        cubie.localScale = scale;
    }

    public void ApplyCubeMove(Move move)
    {
        if (moving != null) { return; }
        moving = StartCoroutine(MovingCube(move, turnDuration));
    }

    private Transform CreateGroup(Move move)
    {
        Vector3 axis = new Vector3(
            Mathf.Abs(move.SignedAxis.x),
            Mathf.Abs(move.SignedAxis.y),
            Mathf.Abs(move.SignedAxis.z)
        );

        float scale = 1f / size;

        Transform group = new GameObject("Group").transform;
        group.parent = transform;
        group.localPosition = axis * (scale * move.LayerIndex - 0.5f + (scale / 2f));
        //group.localPosition = axis * (scale * 1.5f * move.LayerIndex - 0.5f);
        group.localScale = Vector3.one;

        return group;
    }

    private void GroupAxisX(Transform group, int index)
    {
        float treshhold = 1f / size * 0.5f;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Mathf.Abs(child.localPosition.x - group.localPosition.x) < treshhold)
            {
                child.parent = group;
            }
        }
    }

    private void GroupAxisY(Transform group, int index)
    {
        float treshhold = 1f / size * 0.5f;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Mathf.Abs(child.localPosition.y - group.localPosition.y) < treshhold)
            {
                child.parent = group;
            }
        }
    }

    private void GroupAxisZ(Transform group, int index)
    {
        float treshhold = 1f / size * 0.5f;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Mathf.Abs(child.localPosition.z - group.localPosition.z) < treshhold)
            {
                child.parent = group;
            }
        }
    }

    private void Ungroup(Transform group)
    {
        for (int i = group.childCount - 1; i >= 0; i--)
        {
            Transform child = group.GetChild(i);
            child.parent = transform;
        }
    }

    private IEnumerator MovingCube(Move move, float duration, bool isUndoMove = false)
    {
        //Crate temporary rotation transform
        Transform group = CreateGroup(move);

        //Get cubies around group point
        if (move.SignedAxis.x != 0) { GroupAxisX(group, move.LayerIndex); }
        if (move.SignedAxis.y != 0) { GroupAxisY(group, move.LayerIndex); }
        if (move.SignedAxis.z != 0) { GroupAxisZ(group, move.LayerIndex); }

        //Wait for rotation to apply
        yield return WaitForRotation(group, move.SignedAxis, duration);

        //Destroy temporary rotation transform
        Ungroup(group);
        DestroyImmediate(group.gameObject);

        //Add or remove move
        if (isUndoMove)
        {
            moves.Remove(move);
        }
        else
        {
            moves.Add(move);
        }

        moving = null;
    }

    private IEnumerator WaitForRotation(Transform group, Vector3 signedAxis, float duration)
    {
        duration = (Application.isPlaying) ? duration : 0f;

        if (duration > 0f)
        {
            float time = 0f;
            Vector3 step = signedAxis * 90f / duration;

            while (time < duration)
            {
                group.localEulerAngles += step * Time.deltaTime;
                time += Time.deltaTime;
                yield return null;
            }
        }
        
        group.localEulerAngles = signedAxis * 90f;
    }

    public void Shuffle()
    {
        if (shuffling != null) { return; }
        shuffling = StartCoroutine(Shuffling());
    }

    private IEnumerator Shuffling()
    {
        //Clear out previous cube
        GenerateCube();

        //Define random axis list
        List<Vector3Int> axisChoices = new List<Vector3Int>();
        axisChoices.Add(new Vector3Int(1, 0, 0));
        axisChoices.Add(new Vector3Int(-1, 0, 0));
        axisChoices.Add(new Vector3Int(0, 1, 0));
        axisChoices.Add(new Vector3Int(0, -1, 0));
        axisChoices.Add(new Vector3Int(0, 0, 1));
        axisChoices.Add(new Vector3Int(0, 0, -1));

        //Preform x amount of random moves
        for (int i = 0; i < shuffleSteps; i++)
        {
            //Select random axis
            int randomAxisIndex = Random.Range(0, axisChoices.Count);
            Vector3 randomSignedAxis = axisChoices[randomAxisIndex];

            //Select random index
            int randomLayerIndex = Random.Range(0, size);

            //Apply move
            Move randomMove = new Move(randomSignedAxis, randomLayerIndex);
            moving = StartCoroutine(MovingCube(randomMove, 0f));

            //Wait for move to finish
            while (moving != null)
            {
                yield return null;
            }
        }

        shuffling = null;
    }

    public void UndoAllMoves()
    {
        if (moves == null || moves.Count == 0) { return; }
        if (undoing != null) { return; }
        StartCoroutine(UndoingMoves());
    }

    public void UndoLastMove()
    {
        if (moves == null || moves.Count == 0) { return; }
        if (undoing != null) { return; }
        undoing = StartCoroutine(UndoingLastMove());
    }

    private IEnumerator UndoingLastMove()
    {
        //Preform last move backwards
        int lastIndex = moves.Count - 1;
        Move lastMove = moves[lastIndex];
        lastMove.FlipDirection();
        moving = StartCoroutine(MovingCube(lastMove, turnDuration, true));

        //Wait until move is complete
        while (moving != null)
        {
            yield return null;
        }

        undoing = null;
    }

    private IEnumerator UndoingMoves()
    {
        while (moves.Count > 0)
        {
            //Undo last move
            UndoLastMove();

            //Wait until move is complete
            while (undoing != null)
            {
                yield return null;
            }
        }
    }

    public void ResetCube()
    {
        StopAllCoroutines();
        moving = null;
        undoing = null;
        shuffling = null;

        GenerateCube();
    }
}