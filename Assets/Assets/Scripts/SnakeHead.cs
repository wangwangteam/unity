using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/**
 * 挂在蛇头物体上
 * */
public class SnakeHead : MonoBehaviour
{
  
    public List<Transform> bodyList = new List<Transform>();	//body list
    public float velocity = 0.4f;					//移速
    public int step;								//前进步长
    private int x;					//蛇头x
    private int y;					//蛇头y
    private Vector3 headPos;		//蛇头坐标
	private Transform canvas;		//蛇头的容器(此处蛇头是放在canvas里的)
    private bool isDie = false;     //是否死亡
    private int i;
    Vector2 direction = Vector2.up;
    public AudioClip eatClip;		//吃到食物的音效
    public AudioClip dieClip;       //死亡音效
    public GameObject dieEffect;	//死亡特效
    public GameObject bodyPrefab;	//body 预制体
    public Sprite[] bodySprites = new Sprite[2];
   
    void Awake()
    {
        
        canvas = GameObject.Find("Canvas").transform;     
        //通过Resources.Load(string path)方法加载资源，path的书写不需要加Resources/以及文件扩展名 （一般用拖拽的方式）
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("sh", "sh02"));
       // gameObject.GetComponent<Image>().sprite = FoodMaker.Instance.foodSprites[FoodMaker.Instance.index];
        
       
        


    }

    void Start()
    {
        InvokeRepeating("Move", 0, velocity);	//循环调用就可以移动了
        x = 0;y = step;

    }

	/**
	 * 按下上下左右时，蛇头对应旋转方向，同时设置下一次move x,y的变化量
	 * */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && MainUIController.Instance.isPause == false && isDie == false)
        {	//按下空格，加速
            CancelInvoke();
            InvokeRepeating("Move", 0, velocity - 0.2f);
        }
        if (Input.GetKeyUp(KeyCode.Space) && MainUIController.Instance.isPause == false && isDie == false)
        {	//松开空格键，恢复速度
            CancelInvoke();
            InvokeRepeating("Move", 0, velocity);
        }
        if (Input.GetKey(KeyCode.W)||Input.GetKey("up") && y != -step && MainUIController.Instance.isPause == false && isDie == false)
        {
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            x = 0;y = step;
        }
        if (Input.GetKey(KeyCode.S)||Input.GetKey("down") && y != step && MainUIController.Instance.isPause == false && isDie == false)
        {
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, 180);
            x = 0; y = -step;
        }
        if (Input.GetKey(KeyCode.A)||Input.GetKey("left") && x != step && MainUIController.Instance.isPause == false && isDie == false)
        {
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
            x = -step; y = 0;
        }
        if (Input.GetKey(KeyCode.D)||Input.GetKey("right") && x != -step && MainUIController.Instance.isPause == false && isDie == false)
        {
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, -90);
            x = step; y = 0;
        }
        //bodySprites[0] = Resources.Load<Sprite>(PlayerPrefs.GetString("sb01", "sb0201"));   舍弃
        bodySprites[0] = FoodMaker.Instance.foodSprites[FoodMaker.Instance.index];     // 从FoodMaker.cs中获取食物图片的索引赋给蛇身体并更新
        //bodySprites[1] = Resources.Load<Sprite>(PlayerPrefs.GetString("sb02", "sb0202"));   舍弃
        bodySprites[1] = FoodMaker.Instance.foodSprites[FoodMaker.Instance.index];   //同上
    }

	/**
	 * 该游戏的关键算法在于Move()和Grow()
	 * */
    void Move()
    {
        headPos = gameObject.transform.localPosition;                                               //保存下来蛇头移动前的位置
		//此处的移动实现方式：改变transform.localPosition
        gameObject.transform.localPosition = new Vector3(headPos.x + x*1.1f, headPos.y + y*1.1f, headPos.z*1.1f);  //蛇头向期望位置移动
        if (bodyList.Count > 0)
        {
            //由于我们是双色蛇身，此方法弃用
            //bodyList.Last().localPosition = headPos;                                              //将蛇尾移动到蛇头移动前的位置
            //bodyList.Insert(0, bodyList.Last());                                                  //将蛇尾在List中的位置更新到最前
            //bodyList.RemoveAt(bodyList.Count - 1);                                                //移除List最末尾的蛇尾引用

            //由于我们是双色蛇身，使用此方法达到显示目的
            for (int i = bodyList.Count - 2; i >= 0; i--)                                           //从后往前开始移动蛇身
            {
                bodyList[i + 1].localPosition = bodyList[i].localPosition;                          //每一个蛇身都移动到它前面一个节点的位置
            }
            bodyList[0].localPosition = headPos;                                                    //第一个蛇身移动到蛇头移动前的位置
        }
    }

	/**
	 * 
	 * */
    void Grow()
    {
        AudioSource.PlayClipAtPoint(eatClip, Vector3.zero);		//播放音效
        int index = (bodyList.Count % 2 == 0) ? 0 : 1;
		//Quaternion.identity就是指Quaternion(0,0,0,0),就是每旋转前的初始角度,是一个确切的值,而transform.rotation是指本物体的角度,值是不确定的
        GameObject body = Instantiate(bodyPrefab, new Vector3(2000, 2000, 0), Quaternion.identity);
        body.GetComponent<Image>().sprite = bodySprites[index];
        body.transform.SetParent(canvas, false);
        bodyList.Add(body.transform);       //吃到food,body变长，将body加到bodyList中，具体显示交给了Move方法(move方法是循环调用的)
        
    }

    void Die()
    {
        AudioSource.PlayClipAtPoint(dieClip, Vector3.zero);		//死亡音效
        CancelInvoke();		//取消move
        isDie = true;
        Instantiate(dieEffect);	//实例化死亡特效

		//保存此局的数据
        PlayerPrefs.SetInt("lastl", MainUIController.Instance.length);
        PlayerPrefs.SetInt("lasts", MainUIController.Instance.score);
        if (PlayerPrefs.GetInt("bests", 0) < MainUIController.Instance.score)
        {	//成绩好于历史最佳成绩时，才需要更新
            PlayerPrefs.SetInt("bestl", MainUIController.Instance.length);
            PlayerPrefs.SetInt("bests", MainUIController.Instance.score);
        }
        StartCoroutine(GameOver(1.0f));	//1.5s后重新加载该场景
      
    }

    IEnumerator GameOver(float t)
    {
        yield return new WaitForSeconds(t);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

	/**
	 * 吃到食物固定score+5
	 * 吃到奖励 (5,15)随机的score奖励
	 * */
    private void OnTriggerEnter2D(Collider2D collision)
    {
       
		//撞到食物
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);			//食物消失
            MainUIController.Instance.UpdateUI();	//加上5个socre,更新UI的得分
            Grow();		
			FoodMaker.Instance.MakeFood((Random.Range(0, 100) < 20) ? true : false);	//重新生成食物(随机生成奖励)
            
        }
        else if (collision.gameObject.CompareTag("Reward"))
        {
			//吃到奖励
            Destroy(collision.gameObject);
            MainUIController.Instance.UpdateUI(Random.Range(5, 15) * 10);	//score增加，更新ui
            Grow();
   
        }
        else if (collision.gameObject.CompareTag("Body"))	
        {	//碰撞到body,死亡
            Die();
        }
        else
        {
            if (MainUIController.Instance.hasBorder)
            {
                Die();
            }
            else
            {	//没有边界
                switch (collision.gameObject.name)
                {
                    case "Up":	//从最上面对称传到最下面修改y
                        transform.localPosition = new Vector3(transform.localPosition.x, -transform.localPosition.y + 30, transform.localPosition.z);
                        break;
                    case "Down":
                        transform.localPosition = new Vector3(transform.localPosition.x, -transform.localPosition.y - 30, transform.localPosition.z);
                        break;
                    case "Left":	//
                        transform.localPosition = new Vector3(-transform.localPosition.x + 180, transform.localPosition.y, transform.localPosition.z);
                        break;
                    case "Right":
                        transform.localPosition = new Vector3(-transform.localPosition.x + 240, transform.localPosition.y, transform.localPosition.z);
                        break;
                }
            }
        }
    }
}
