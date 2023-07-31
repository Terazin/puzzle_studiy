using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayDirector : MonoBehaviour
{
    /*struct FallData 
    { 
    public readonly int X { get; }
        public readonly int Y { get; }
        public readonly int Dest { get; }

        public FallData(int x, int y, int dest) 
        {
            X = x;
            Y = y;
            Dest = dest;
        }
    }*/
    

    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogicalInput _logicalInput = new();
    BoardController _boardController = default!;

    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };// ��next�̃Q�[���I�u�W�F�N�g�̐���

    interface IState
    {
        public enum E_State
        {
            Control = 0,
            GameOver = 1,
            Falling = 2,

            MAX,

            Unchanged,
        }

        E_State Initialize(PlayDirector parent);
        E_State Update(PlayDirector parent);
    }

   IState.E_State _current_state = IState.E_State.Falling;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _boardController = GetComponent<BoardController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        //Spawn(_nextQueue.Update());
        //UpdateNextsView();

        InitializeState();
    }

    void UpdateNextsView()
    {
        _nextQueue.Each((int idx, Vector2Int n) => {
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
    }

    //IState.E_State _current_state = IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new GameOverState(),
        new FallingState(),
    };
    void InitializeState() 
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);
        var next_state = states[(int)_current_state].Initialize(this);
        if (next_state != IState.E_State.Unchanged) 
        {
            _current_state = next_state;
            InitializeState();
        }
    }
    void UpdateState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);
        var next_state = states[(int)_current_state].Update(this);
        if (next_state != IState.E_State.Unchanged)
        {
            _current_state = next_state;
            InitializeState();
        }
    }

    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogicalInput.Key.MAX]{
        KeyCode.RightArrow, // Right
        KeyCode.LeftArrow,  // Left
        KeyCode.X,          // RotR
        KeyCode.Z,          // RotL
        KeyCode.UpArrow,    // QuickDrop
        KeyCode.DownArrow,  // Down
    };

    // ���͂���荞��
    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;// �f�o�C�X�l

        // �L�[���͎擾
        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
                //�ύX
            }
        }

        _logicalInput.Update(inputDev);
    }

    
    // Update is called once per frame

    void FixedUpdate()
    {
        // ���͂���荞��
        UpdateInput();
        UpdateState();

        if (!player.activeSelf)
        {
            Debug.Log("aaa");
            Spawn(_nextQueue.Update());
            UpdateNextsView();
        }


      
    }

    class ControlState : IState 
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            if (!parent.Spawn(parent._nextQueue.Update())) 
            {
                return IState.E_State.GameOver;
            }
            parent.UpdateNextsView();
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent) 
        {
            return parent.player.activeSelf ? IState.E_State.Unchanged : IState.E_State.Falling;
            //control => Falling
        }

    }

    class GameOverState : IState 
    {
        public IState.E_State Initialize(PlayDirector parent) 
        {
            SceneManager.LoadScene(0);
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent) 
        { return IState.E_State.Unchanged; }

    }
    class FallingState : IState 
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            return parent._boardController.CheckFall() ?IState.E_State.Unchanged:IState.E_State.Control;
        }
        public IState.E_State Update(PlayDirector parent) 
        {
            return parent._boardController.Fall() ? IState.E_State.Unchanged : IState.E_State.Control;
        }
    }

 bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
   
}