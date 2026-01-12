using Firebase;//기본
using Firebase.Auth;
using Firebase.Database;//파이어베이스 데이터베이스를 임포트하면 쓸 수 있는 DB 기능
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;//비동기를 위해 사용
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseAuthManager : Singleton<FirebaseAuthManager>
{
    FirebaseAuth auth; // 인증 진행을 위한 객체
    public FirebaseUser user; // 인증 후 인증된 유저 정보를 들고 있게 하는 것
    public DatabaseReference dbRef;//DB에 대한 저옵를 여러 씬에서 다양하게 쓰려고 스테틱 처리

    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField pwField;
    [SerializeField] TMP_InputField nickField;

    Coroutine onCheckEmail;
    void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;//비동기 작업을 기억
            if (dependencyStatus == Firebase.DependencyStatus.Available)//가능하다는 결과 받았다면
            {
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;//인증 정보 기억
                loginButton.interactable = true;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                //실패시 로그 띄움
                UnityEngine.Debug.LogError(System.String.Format("뭔가 잘못되었음"));
            }
        }
        );

        if (nickField != null)
        {
            nickField.interactable = false;
        }
    }
    private void Start()
    {
        if (loginButton != null)
        {
            loginButton.gameObject.SetActive(false);
        }
    }

    public void Login()
    {
        StartCoroutine(LoginCoroutine(emailField.text, pwField.text));
    }

    public void Register()
    {
        StartCoroutine(RegisterCoroutine(emailField.text, pwField.text, nickField.text));
    }

    IEnumerator RegisterCoroutine(string email, string password, string UserName)
    {
        Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);
        if (RegisterTask.Exception != null)
        {
            Debug.LogWarning(message: "실패 사유" + RegisterTask.Exception);
            FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "회원가입 실패";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일 누락";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드 누락";
                    break;
                case AuthError.WeakPassword:
                    message = "패스워드 약함";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "중복 이메일";
                    break;
                default:
                    message = "기타 사유. 관리자 문의 바람";
                    break;
            }
            Debug.Log(message);
        }
        else//생성 완료
        {
            user = RegisterTask.Result.User;
            if (user != null)
            {

                //이건 로컬에서 만든 것
                UserProfile profile = new UserProfile { DisplayName = UserName };

                Task profileTask = user.UpdateUserProfileAsync(profile);
                //predicate : 참거짓을 판단하는 함수에 저걸 넣겠다
                yield return new WaitUntil(predicate: () => profileTask.IsCompleted);
                //여기서 닉네임에 욕 들어가면 차단하도록
                if (profileTask.Exception != null)
                {
                    Debug.LogWarning("닉네임 설정 실패 " + profileTask.Exception);
                    FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                }
                else
                {
                    loginButton.interactable = true;
                }
            }
            Debug.Log("회원가입 성공 " + user.DisplayName + "님 환영합니다");
        }

        SceneManager.LoadSceneAsync("Lobby");
    }

    public void CheckEmail()
    {
        // 이메일 형식이 최소한의 틀을 갖췄을 때만 체크 시작
        if (emailField.text.Contains("@") && emailField.text.Contains("."))
        {
            if (onCheckEmail != null)
                StopCoroutine(onCheckEmail);
            onCheckEmail = StartCoroutine(OnCheckEmail(emailField.text));
        }

    }

    IEnumerator OnCheckEmail(string email)
    {
        //해당 방법은 이메일 열거 보호를 해제해야 하므로 제외 <- 우회하려고 회원가입 과정, 로그인 과정 중 발생하는 오류 코드 까지 사용해 보았으나 실패
        var task = auth.FetchProvidersForEmailAsync(email);
        yield return new WaitUntil(() => task.IsCompleted);

        //Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(email, "FakePasswordForTest000001");
        //yield return new WaitUntil(() => LoginTask.IsCompleted);
        //bool checkEmail = false;
        //if (LoginTask.Exception != null)
        //{
        //    FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
        //    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;//진짜 우리가 해석 가능한 형태로 바꿈

        //    string message = "";
        //    switch (errorCode)
        //    {
        //        case AuthError.MissingEmail:
        //            message = "이메일 누락";
        //            break;
        //        case AuthError.MissingPassword:
        //            message = "패스워드 누락";
        //            break;
        //        case AuthError.WrongPassword:
        //            message = "패스워드 틀림";
        //            break;
        //        case AuthError.InvalidEmail:
        //            message = "이메일 형식이 옳지 않음";
        //            break;
        //        case AuthError.UserNotFound:
        //            message = "아이디가 존재하지 않음";
        //            break;
        //        default:
        //            message = "관리자에게 문의 바랍니다";
        //            break;
        //    }
        //    Debug.Log(message);
        //}

        bool checkEmail = task.Result.Any();

        loginButton.gameObject.SetActive(checkEmail);
        registerButton.gameObject.SetActive(!checkEmail);
        nickField.interactable = !checkEmail;
        if (nickField.interactable == false)
        {
            nickField.text = "";
        }
    }

    IEnumerator LoginCoroutine(string email, string password)
    {

        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        if (LoginTask.Exception != null) // 로그인에서 문제가 발생했을 때 Exception에 담김
        {
            Debug.Log("다음과 같은 이유로 로그인 실패" + LoginTask.Exception);
            //파이어베이스에선, 에러를 파이어베이스 형식으로 해석할 수 있게 클래스 제공
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;//진짜 우리가 해석 가능한 형태로 바꿈
            string message = "";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일 누락";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드 누락";
                    break;
                case AuthError.WrongPassword:
                    message = "패스워드 틀림";
                    break;
                case AuthError.InvalidEmail:
                    message = "이메일 형식이 옳지 않음";
                    break;
                case AuthError.UserNotFound:
                    message = "아이디가 존재하지 않음";
                    break;
                default:
                    message = "관리자에게 문의 바랍니다";
                    break;
            }
        }
        else//여기 왔단 뜻은 성공
        {
            user = LoginTask.Result.User;//로그인 잘 되었으니, 유저 정보 기억
            nickField.text = user.DisplayName;//파이어베이스 상에 기억된 닉네임 가져옴
            loginButton.interactable = true;
            Debug.Log("로그인 성공");
            SceneManager.LoadSceneAsync("Lobby");
        }
    }


    public IEnumerator ChangeNickName(string nickname, TMP_InputField nicknameField)
    {
        RefreshUser();
        UserProfile profile = new UserProfile
        { DisplayName = nickname };
        Task task = user.UpdateUserProfileAsync(profile);
        //predicate : 참거짓을 판단하는 함수에 저걸 넣겠다
        yield return new WaitUntil(predicate: () => task.IsCompleted);
        if (task.IsCanceled)
        {
            Debug.LogError("업데이트 취소");
            yield break;
        }
        if (task.IsFaulted)
        {
            Debug.LogError("업데이트 실패");
            yield break;
        }
        //유저 갱신
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("닉네임 변경 성공: " + user.DisplayName);
        nicknameField.placeholder.GetComponent<TMP_Text>().text = user.DisplayName;
        nicknameField.text = "";

        PhotonNetwork.NickName = FirebaseAuthManager.Instance.user.DisplayName;
    }

    public void RefreshUser()
    {
        user= FirebaseAuth.DefaultInstance.CurrentUser;
    }
}
