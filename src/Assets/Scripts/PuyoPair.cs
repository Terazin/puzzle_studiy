using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoPair : MonoBehaviour
{
    [SerializeField] PuyoController[] puyos = { default!, default! };

    // Start is called before the first frame update
    public void SetPuyoType(PuyoType axis, PuyoType Child)
    {
        puyos[0].SetPuyoType(axis);
        puyos[1].SetPuyoType(Child);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
