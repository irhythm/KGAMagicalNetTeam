using Firebase; //기본 기능들
using Firebase.Auth; //인증정보 기능들
using Firebase.Database; //DB기능들
using System.Collections; //비동기 작업 할거라 그럼
using System.Threading.Tasks;
using System.Collections.Generic; //콜렉션에 담아서 한번에 보내려고 선언
using UnityEngine;
using UnityEngine.UI;


public class FirebaseDBMgr : MonoBehaviour
{
    DatabaseReference dbRef; //이건 껍데기
    public FirebaseUser user;

    [SerializeField] InputField moneyField;
    [SerializeField] InputField xpField;
    [SerializeField] InputField levelField;

    const string Inven = "Inventory";

    public List<string> inventory = new List<string> { "테스트아이템1", "테스트아이템2", "테스트아이템3" };
    public Dictionary<string, int> inventoryDicVer = new Dictionary<string, int>()
    {
        {"딕셔너리테스트1", 1 },
        {"딕셔너리테스트2", 3 },
        {"딕셔너리테스트3", 6 },
    };

    void Start()
    {
        this.dbRef = FirebaseAuthManager.Instance.dbRef;
        this.user = FirebaseAuthManager.Instance.user;
    }

    public void SaveInventoryandDict()
    {
        dbRef.Child("users").Child(user.UserId).Child("Inventory").SetValueAsync(inventory); //배열계열은 이렇게 딸깍


        //▼딕셔너리는 오브젝트로 변환해서 올리는 작업 필요
        Dictionary<string, object> invenDicObj = new Dictionary<string, object>();
        foreach (var kvp in inventoryDicVer)
        { 
            invenDicObj[kvp.Key] = kvp.Value;
        }
        dbRef.Child("users").Child(user.UserId).Child("InventoryDict").SetValueAsync(invenDicObj);
    }

    IEnumerator LoadInventoryCoroutine()
    {
        var DBTask = dbRef.Child("users").Child(user.UserId).Child("Inventory").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning("인벤토리 불러오기 실패" + DBTask.Exception);
        }
        else if (DBTask.Result.Value == null)
        {
            Debug.LogWarning("인벤토리가 비었습니다");
        }
        else
        {
            inventory.Clear(); //기존 로컬상의 인벤토리 깔끔하게 날리고
            foreach (DataSnapshot item in DBTask.Result.Children) //Child는 하나, Children은 여러 차일드
            {
                inventory.Add(item.Value.ToString());
            }
            Debug.Log("인벤토리 로드 완료" + string.Join(", ", inventory));

        }
    }

    IEnumerator LoadInventoryDictCoroutine()
    {
        var DBTask = dbRef.Child("users").Child(user.UserId).Child("InventoryDict").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"인벤토리 딕셔너리 불러오기 실패! 사유: {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            Debug.Log("인벤토리 딕셔너리에 아이템이 없습니다.");
        }
        else
        {
            inventoryDicVer.Clear();

            foreach (DataSnapshot item in DBTask.Result.Children)
            {
                string key = item.Key;

                if(int.TryParse(item.Value.ToString(), out int value))
                {
                    inventoryDicVer[key] = value;
                }
                else
                {
                    Debug.LogWarning($"딕셔너리 값 변환 실패: {item.Key} = {item.Value}");
                }
            }
            Debug.Log($"인벤토리 딕셔너리 로드 완료: {string.Join(", ", inventoryDicVer)}");
        }
    }

    public void LoadInventory()
    {
        StartCoroutine(LoadInventoryCoroutine());
        StartCoroutine(LoadInventoryDictCoroutine());
    }

    public void SaveToDb()
    {
        //StartCoroutine(UpdateMoney(int.Parse(moneyField.text)));
        //StartCoroutine(UpdateLevel(int.Parse(levelField.text)));
        //StartCoroutine(UpdateExp(int.Parse(xpField.text)));
        SaveInventoryandDict();
    }
    
    public void LoadFromDb()
    {
        StartCoroutine(LoadUserData());
    }

    IEnumerator LoadUserData()
    {
        var DBTask = dbRef.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(()=>DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning("데이터 불러오기 실패" + DBTask.Exception);
        }
        else if (DBTask.Result.Value == null)
        {
            Debug.LogWarning("저장된 데이터가 없습니다");
        }
        else
        {
            DataSnapshot snapShot = DBTask.Result; //결과는 덩어리로 기억되어있으니, 이걸 나중에 분석하고자 큰 틀에 담아둠
            moneyField.text = snapShot.Child("money").Exists ? snapShot.Child("money").Value.ToString() : "0";
            xpField.text = snapShot.Child("exp").Exists ? snapShot.Child("exp").Value.ToString() : "0";
            levelField.text = snapShot.Child("level").Exists ? snapShot.Child("level").Value.ToString() : "0";
            Debug.Log("여기까진 로드 잘 성공함");
        }


    }





    IEnumerator UpdateMoney(int money)
    {
        //마치 속에 user 라는 이름의 폴더를 만들듯, 
        var DBTask = dbRef.Child("users").Child(user.UserId).Child("money").SetValueAsync(money);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"돈 업데이트 실패! 사유 {DBTask.Exception}");
        }
        else
        {
            //만약 저장완료 팝업같은거 띄우고 싶다면 여기 작성
            //저장완료를 3초간 띄우고, 페이드아웃
        }
    }

    IEnumerator UpdateExp(int exp)
    {
        //마치 속에 user 라는 이름의 폴더를 만들듯, 
        var DBTask = dbRef.Child("users").Child(user.UserId).Child("exp").SetValueAsync(exp); //2개 바꾸기 exp로
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"경험치 업데이트 실패! 사유 {DBTask.Exception}"); //경험치로 바꾸기
        }
        else
        {
            //만약 저장완료 팝업같은거 띄우고 싶다면 여기 작성
            //저장완료를 3초간 띄우고, 페이드아웃
        }
    }

    IEnumerator UpdateLevel(int lvl)
    {
        //마치 속에 user 라는 이름의 폴더를 만들듯, 
        var DBTask = dbRef.Child("users").Child(user.UserId).Child("lvl").SetValueAsync(lvl); //2개 바꾸기 exp로
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"레벨 업데이트 실패! 사유 {DBTask.Exception}"); //레벨로 바꾸기
        }
        else
        {
            //만약 저장완료 팝업같은거 띄우고 싶다면 여기 작성
            //저장완료를 3초간 띄우고, 페이드아웃
        }
    }







}
