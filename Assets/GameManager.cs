using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets;
using System.IO;
//이건 스레기..

using BoardKey = System.ValueTuple<string, int>;

public class CPU
{
    

    public EnumData.EBoardState maker = EnumData.EBoardState.E;
    public float epsilon = 0.5f;

    private float learning_rate = 0.1f;
    private float gamma = 0.9f;
    private System.Random random = new System.Random();
    public Dictionary<BoardKey, float> qtable = new Dictionary<BoardKey, float>();
    public int Choice<T>(List<T> index_list, List<float> prob_list)
    {
        if( index_list.Count != prob_list.Count)
		{
            return -1;
		}

        List<float> pdf = new List<float>();
        float sum_pdf = 0f;
        foreach( float prob in prob_list )
		{
            sum_pdf += prob;
            pdf.Add(sum_pdf);
            
        }

        float randomvalue = (float)random.NextDouble();
        int choice = -1;
        for(int i = 0; i < pdf.Count; ++i)
		{
            if(randomvalue <= pdf[i])
			{
                choice = i;
                break;
			}
		}

        return choice;
    }


    public int policy(Environment env)
	{
        //현재 보드 상태와 해야 할 액션 경우의 수 저장
        List<int> available_action = env.GetAvailableIndex();
        List<float> qvalues = new List<float>();
        for(int i = 0; i < available_action.Count; ++i)
		{
            qvalues.Add(0);
        }
        
        for( int i = 0; i < available_action.Count; ++i)
		{


            BoardKey key = new BoardKey(env.GetBoardState(), available_action[i]);

            if (qtable.ContainsKey(key) == false)
            {
                qtable.Add(key, 0);
            }

            qvalues[i] = qtable[key];

        }

        float greedy_action = qvalues.Max();
        

        //값은 값이 여러개 있는지 확인후 double check에 상태를 저장
        List<float> double_check = new List<float>();
        for(int i = 0; i <qvalues.Count; ++i)
		{
            double_check.Add(0);
        }


        List<float> greedy_list = new List<float>();


        for(int i = 0; i < qvalues.Count; ++i)
		{
            if(qvalues[i] == greedy_action)
			{
                double_check[i] = 1;

            }
			else
			{
                double_check[i] = 0;
            }
		}

        float sum = double_check.Sum();
        if (sum > 1)
		{
            for(int i = 0; i < double_check.Count; ++i)
			{
                double_check[i] = (float)(double_check[i] / sum);
            }
		}

        int greedy_action_choice = Choice(double_check, double_check);
        
        List<float> probability_list = new List<float>();
        for( int i = 0; i < available_action.Count; ++i)
		{
            probability_list.Add(0f);
		}

        for( int i = 0; i < available_action.Count; ++i)
		{
            if( i == greedy_action_choice)
            {
                probability_list[i] = 1 - epsilon + (epsilon / available_action.Count);
			}
			else
			{
                probability_list[i] = epsilon / available_action.Count;
			}
		}

        int action_choice = Choice(available_action, probability_list);


        return available_action[action_choice];

	}

    public int GetIndex(Environment env)
	{
        List<int> availableIndex = env.GetAvailableIndex();
        int random_index = UnityEngine.Random.Range(0, availableIndex.Count);
        int index = availableIndex[random_index];
        return index;
	}

    public void learn_qtable(BoardKey key, Environment env, float reward)
	{

        //끝난 상태
        if(env.GetCurrentBoardState() != EnumData.EBoardResult.Continue)
		{
            if(qtable.ContainsKey(key) == false)
			{
                qtable.Add(key, 0f);
			}

            float qtable_value = qtable[key];
            float reward_value = reward - qtable_value;
            qtable[key] += learning_rate * reward_value;
            float qvalue = qtable[key];
            return;
		}
        else
		{
            List<int> available_action = env.GetAvailableIndex();
            List<float> qvalues = new List<float>();

            for(int i = 0; i < available_action.Count; ++i)
			{
                qvalues.Add(0);
            }

            for(int i = 0; i < available_action.Count; ++i)
			{
                BoardKey next_key = new BoardKey(env.GetBoardState(), available_action[i]);

                if(qtable.ContainsKey(next_key) == false)
				{
                    qtable.Add(next_key, 0f);
				}

                qvalues[i] = qtable[next_key];

            }

            float max_qvalue = qvalues.Max();



            qtable[key] += learning_rate * (reward + gamma * max_qvalue - qtable[key]);

        }

        
	}
    
}

public class Environment
{
    public List<EnumData.EBoardState> Board = new List<EnumData.EBoardState>();
    public EnumData.EBoardResult current_state = EnumData.EBoardResult.Continue; 
    public void Initilize()
	{
        for(int i = 0; i < 9; ++i)
		{
            Board.Add(0);
        }
	}

    public void ResetBoard()
	{
        for (int i = 0; i < 9; ++i)
        {
            Board[i] = EnumData.EBoardState.E;
        }
    }


    public string GetBoardState()
    {
        string board_state = string.Join("", Board.ToArray());
        return board_state;

    }


    public List<int> GetAvailableIndex()
	{
        List<int> available_list = new List<int>();

        for( int i = 0; i < Board.Count; ++i)
		{
            if( Board[i] == EnumData.EBoardState.E )
			{
                available_list.Add(i);
			}
		}
        return available_list;
    }
      
    public EnumData.EBoardResult GetCurrentBoardState()
	{
        return current_state;

    }
    public EnumData.EBoardResult Move(EnumData.EBoardState maker, int Position)
	{
		Board[Position] = maker;
		bool result = is_check_end();
		if (result == true)
		{
            if(maker == EnumData.EBoardState.O)
			{
                current_state = EnumData.EBoardResult.O_Win;
                return EnumData.EBoardResult.O_Win;
			}
            else
			{
                current_state = EnumData.EBoardResult.X_Win;
                return EnumData.EBoardResult.X_Win;
			}
        }

        if (GetAvailableIndex().Count == 0)
		{
            current_state = EnumData.EBoardResult.Draw;
            return EnumData.EBoardResult.Draw;
		}

        current_state = EnumData.EBoardResult.Continue;
        return EnumData.EBoardResult.Continue;
	}

	public bool is_check_end()
	{
        if( (Board[0] == Board[1]) && (Board[1] == Board[2]) && (Board[0] != EnumData.EBoardState.E) ||
            (Board[3] == Board[4]) && (Board[4] == Board[5]) && (Board[3] != EnumData.EBoardState.E) ||
            (Board[6] == Board[7]) && (Board[7] == Board[8]) && (Board[6] != EnumData.EBoardState.E) ||
            (Board[0] == Board[3]) && (Board[3] == Board[6]) && (Board[0] != EnumData.EBoardState.E) ||
            (Board[1] == Board[4]) && (Board[4] == Board[7]) && (Board[1] != EnumData.EBoardState.E) ||
            (Board[2] == Board[5]) && (Board[5] == Board[8]) && (Board[2] != EnumData.EBoardState.E) ||
            (Board[0] == Board[4]) && (Board[4] == Board[8]) && (Board[0] != EnumData.EBoardState.E) ||
            (Board[6] == Board[4]) && (Board[4] == Board[2]) && (Board[6] != EnumData.EBoardState.E) )
		{
            
            return true;
		}
        return false;

    }

}


public class GameManager : MonoBehaviour
{
    public Button Resetbutton;
    private Text ResetbuttonText;

    public Button Learningbutton;
    private Text LearningbuttonText;

    public Button Savebutton;

    public Text TurnTextUI;
    public Text LeftTextUI;
    public Text RightTextUI;

    public Text OLabelUI;
    public Text XLabelUI;
    public Text DrawLabelUI;

    public Text O_ValueText;
    public int O_WinCount;

    public Text X_ValueText;
    public int X_WinCount;

    public Text Draw_ValueText;
    public int DrawCount;


    private Environment enviorment_tictactoe;
    private Environment environment_CPU;

    private GameObject TilePrefab;
    private GameObject O_Prefab;
    private GameObject X_Prefab;

    private CPU cpu1;
    private CPU cpu2;

    private bool playing = true;
    private GameObject O_tile;

    private List<Tile> tile_renderer_list = new List<Tile>();

    private BoardKey cpu1_backup_key;
    private BoardKey cpu2_backup_key;



    private bool Learning = false;
    private int pointlimit = 20;
    private string LearningText = "Learning";
    private int current_point = 0;


    private bool first = true;

    public bool release = true;

    private void LoadResources() 
    {
        TilePrefab = Resources.Load("Tile") as GameObject;
        O_Prefab = Resources.Load("O") as GameObject;
        X_Prefab = Resources.Load("X") as GameObject;

        Vector3 Pos = new Vector3(0, 0, 0);
        O_tile = GameObject.Instantiate(O_Prefab, Pos, transform.rotation);
        SpriteRenderer Renderer = O_tile.GetComponent<SpriteRenderer>();
        Color alpha = Renderer.color;
        alpha.a = 0.1f;
        Renderer.color = alpha;
        O_tile.name = "temp O tile";
        O_tile.SetActive(false);





    }

	public void Update()
	{

		if(Learning == false)
		{
            return;
		}

        //러닝 ㄱㄱ
        string str_point = "";
        for ( int i = 0; i < current_point; ++i)
		{
            str_point += ".";
        }

        TurnTextUI.text = LearningText + str_point;

        if( pointlimit < current_point)
		{
            current_point = 0;
        }
		else
		{
            current_point += 1;
		}

        int policy_choice_cpu1 = cpu1.policy(environment_CPU);
        string board_state_cpu1 = environment_CPU.GetBoardState();
        BoardKey cpu1_backup_key = new BoardKey(board_state_cpu1, policy_choice_cpu1);
        EnumData.EBoardResult cpu1_result = environment_CPU.Move(cpu1.maker, policy_choice_cpu1);
        tile_renderer_list[policy_choice_cpu1].Renderer(cpu1.maker);

        if(cpu1_result == EnumData.EBoardResult.Draw)
		{
            cpu1.learn_qtable(cpu1_backup_key, environment_CPU, 0);
            cpu2.learn_qtable(cpu2_backup_key, environment_CPU, 0);
            ResetRendererBoard();
            environment_CPU.ResetBoard();
            DrawCount += 1;
            return;
        }
        else if(cpu1_result == EnumData.EBoardResult.O_Win)
		{
            cpu1.learn_qtable(cpu1_backup_key, environment_CPU, 1);
            cpu2.learn_qtable(cpu2_backup_key, environment_CPU, -1);
            ResetRendererBoard();
            environment_CPU.ResetBoard();
            O_WinCount += 1;
            return;
        }

        if(first == false)
		{
            cpu2.learn_qtable(cpu2_backup_key, environment_CPU, -0.01f);
        }
        first = false;

        int policy_choice_cpu2 = cpu2.policy(environment_CPU);
        string board_state_cpu2 = environment_CPU.GetBoardState();
        cpu2_backup_key = new BoardKey(board_state_cpu2, policy_choice_cpu2);
        EnumData.EBoardResult cpu2_result = environment_CPU.Move(cpu2.maker, policy_choice_cpu2);
        tile_renderer_list[policy_choice_cpu2].Renderer(cpu2.maker);

        if (cpu2_result == EnumData.EBoardResult.Draw)
        {
            cpu1.learn_qtable(cpu1_backup_key, environment_CPU, 0);
            cpu2.learn_qtable(cpu2_backup_key, environment_CPU, 0);
            ResetRendererBoard();
            environment_CPU.ResetBoard();
            DrawCount += 1;
            return;
        }
        else if (cpu2_result == EnumData.EBoardResult.X_Win)
        {
            cpu1.learn_qtable(cpu1_backup_key, environment_CPU, -1);
            cpu2.learn_qtable(cpu2_backup_key, environment_CPU, 1);
            ResetRendererBoard();
            environment_CPU.ResetBoard();
            X_WinCount += 1;
            return;
        }

        cpu1.learn_qtable(cpu1_backup_key, environment_CPU, -0.01f);

        O_ValueText.text = O_WinCount.ToString();
        X_ValueText.text = X_WinCount.ToString();
        Draw_ValueText.text = DrawCount.ToString();

    }

	private void RenderBackground()
	{
        int index = 0;

        for (int y = 0; -3 < y; --y)
        {
            for (int x = 0; x < 3; ++x)
            {
                Vector3 Pos = new Vector3(x, y, 0);
                GameObject tile = GameObject.Instantiate(TilePrefab, Pos, transform.rotation);
                tile.name = index.ToString();
				Tile tile_script = tile.GetComponent<Tile>();
                tile_renderer_list.Add(tile_script);
                tile_script.PosX = x;
                tile_script.PosY = y;
                tile_script.index = index;
                tile_script.eventMouseEnter = OnEnterTile;
                tile_script.eventMouseExit = OnOverTile;
                tile_script.eventMouseClick= OnClickTile;
                index += 1;
            }
        }
    }
    private void SaveQtable()
	{
        string path = Directory.GetCurrentDirectory() + "\\cpu1.txt";
        File.WriteAllText(path, Assets.Utility.Save_QTable(cpu1.qtable));

        string path2 = Directory.GetCurrentDirectory() + "\\cpu2.txt";
        File.WriteAllText(path2, Assets.Utility.Save_QTable(cpu2.qtable));

        Savebutton.gameObject.SetActive(false);
    }

    private void InitilizeUI()
	{
        ResetbuttonText = Resetbutton.GetComponentInChildren<Text>();
        ResetbuttonText.text = "Regame";
        Resetbutton.onClick.AddListener(OnClickResetButton);

        LearningbuttonText = Learningbutton.GetComponentInChildren<Text>();
        LearningbuttonText.text = "Learn";
        Learningbutton.onClick.AddListener(OnClickLearnButton);

        Savebutton.onClick.AddListener(SaveQtable);



        TurnTextUI.text = "Turn text render";
    }

    private void Initialize() 
    {
        LoadResources();
        
        InitilizeUI();
        RenderBackground();


        //game
        enviorment_tictactoe = new Environment();
        enviorment_tictactoe.Initilize();

        environment_CPU = new Environment();
        environment_CPU.Initilize();



        //Learning
        cpu1 = new CPU();
        cpu1.maker = EnumData.EBoardState.O;
        cpu2 = new CPU();
        cpu2.maker = EnumData.EBoardState.X;


        if (release == true)
        {
            OLabelUI.gameObject.SetActive(false);
            XLabelUI.gameObject.SetActive(false);
            DrawLabelUI.gameObject.SetActive(false);

            O_ValueText.gameObject.SetActive(false);
            X_ValueText.gameObject.SetActive(false);
            Draw_ValueText.gameObject.SetActive(false);

            Learningbutton.gameObject.SetActive(false);

            Savebutton.gameObject.SetActive(false);

            //TextAsset cpu1_text = Resources.Load("cpu1") as TextAsset;
            TextAsset cpu2_text = Resources.Load("cpu2") as TextAsset;


            //cpu1.qtable = Utility.Load_Qtable(cpu1_text.text);
            cpu2.qtable = Utility.Load_Qtable(cpu2_text.text);

            cpu2.epsilon = 0;
        }

    }

    public void Awake()
    {
        Initialize();
    }


	private void OnEnterTile(int PosX, int PosY , int index)
	{
        if( playing == false)
		{
            return;
		}

        if( enviorment_tictactoe.Board[index] != 0)
		{
            return;   
        }

        O_tile.SetActive(true);
        O_tile.transform.position = new Vector3(PosX, PosY, 0);
    }

    private void OnOverTile(int PosX, int PosY , int index)
    {
        if (playing == false)
        {
            return;
        }

        if (enviorment_tictactoe.Board[index] != 0)
		{
            return;
		}

        O_tile.SetActive(false);
    }

    private void OnClickTile(int PosX, int PosY ,int index)
    {
        if (playing == false)
        {
            return;
        }

        if (enviorment_tictactoe.Board[index] != 0)
        {
            return;
        }

        TurnTextUI.text = "Your Turn";

        tile_renderer_list[index].Renderer(EnumData.EBoardState.O);
        EnumData.EBoardResult result = enviorment_tictactoe.Move(EnumData.EBoardState.O, index);

        switch (result)
        {
            case EnumData.EBoardResult.Continue:
                {
                    break;
                }
            case EnumData.EBoardResult.Draw:
                {
                    playing = false;
                    TurnTextUI.text = "Draw";
                    return;
                }
            case EnumData.EBoardResult.O_Win:
                {
                    playing = false;
                    TurnTextUI.text = "O Win";
                    return;
                }
            case EnumData.EBoardResult.X_Win:
                {
                    playing = false;
                    TurnTextUI.text = "X Win";
                    return;
                }

        }


        result = CPUTurn();

        switch (result)
        {
            case EnumData.EBoardResult.Continue:
                {
                    break;
                }
            case EnumData.EBoardResult.Draw:
                {
                    playing = false;
                    TurnTextUI.text = "Draw";
                    return;
                }
            case EnumData.EBoardResult.O_Win:
                {
                    playing = false;
                    TurnTextUI.text = "O Win";
                    return;
                }
            case EnumData.EBoardResult.X_Win:
                {
                    playing = false;
                    TurnTextUI.text = "X Win";
                    return;
                }

        }
    }

    private EnumData.EBoardResult CPUTurn()
	{
        //CPU ㄱㄱ
        int cpu_choice = cpu2.policy(enviorment_tictactoe);
        tile_renderer_list[cpu_choice].Renderer(EnumData.EBoardState.X);
        return enviorment_tictactoe.Move(EnumData.EBoardState.X, cpu_choice);

	}

	private void ResetRendererBoard()
	{
		for (int i = 0; i < 9; ++i)
		{
			tile_renderer_list[i].Renderer(EnumData.EBoardState.E);
		}
	}

	private void OnClickResetButton()
    {
        cpu2.epsilon = 0f;
        ResetRendererBoard();
        enviorment_tictactoe.ResetBoard();
        playing = true;
    }

    private void OnClickLearnButton()
    {
        if (Learning == false)
        {
            playing = false;
            Learning = true;

            LearningbuttonText.text = "Stop";
            LeftTextUI.text = "CPU O";
            RightTextUI.text = "CPU X";
            cpu2.epsilon = 0.5f;
        }
        else
		{
            playing = true;
            Learning = false;
            LearningbuttonText.text = "Learn";

            LeftTextUI.text = "Human O";
            RightTextUI.text = "CPU X";
            cpu2.epsilon = 0f;
            OnClickResetButton();
        }

    }
}
