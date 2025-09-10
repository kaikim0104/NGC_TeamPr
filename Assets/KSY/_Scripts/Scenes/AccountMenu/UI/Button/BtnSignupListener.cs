using BackEnd;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BtnSignupListener : MonoBehaviour
{
    [SerializeField] private TMP_InputField input_ID;
    [SerializeField] private TMP_InputField input_Pw;
    [SerializeField] private TMP_InputField input_Nickname;
    public void Signup()
    {
        string id = input_ID.text;
        string pw = input_Pw.text;
        string nickname = input_Nickname.text;

        //입력받은 닉네임이 빈 칸(공백 포함)일 경우 처리
        if(string.IsNullOrWhiteSpace(nickname))
        {
            Debug.Log("닉네임이나 아이디를 모두 입력해주세요.");
            //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "121212");
            return;
        }

        //회원 가입 시도
        int signup_statusCode = Server.Instance.TrySignup(id, pw);

        switch(signup_statusCode)
        {
            //회원 가입 성공 처리
            case 201:
                {
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "Registration successful");
                    break;
                }
            //빈칸 예외처리
            case 400:
                {
                    //text : 아이디와 비밀번호를 모두 칸에 적어주세요.
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "22222");
                    break;
                }
            //아이디 중복 처리
            case 409:
                {
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "11111");
                    break;
                }
            //그 밖에 예외처리
            default:
                {
                    //text : Error ! 다시 시도해주세요.
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "33333");
                    break;
                }
        }

        //닉네임 변경 시도
        int updateNicknameStatuscode = Server.Instance.TryUpdateNickname(nickname);

        switch (updateNicknameStatuscode)
        {
            //닉네임 변경 성공 처리
            case 204:
                {
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "Registration successful");
                    break;
                }
            //닉네임이 20자 이상일 경우 처리
            case 400:
                {
                    Debug.LogError("닉네임이 너무 깁니다. 다른 닉네임을 선택해주세요.");
                    break;
                }
            //이미 중복된 닉네임이 있는 경우 처리
            case 409:
                {
                    Debug.LogError("중복된 닉네임입니다. 다른 닉네임을 선택해주세요.");
                    break;
                }
            //그 밖에 예외처리
            default:
                {
                    //text : Error ! 다시 시도해주세요.
                    //UIManager.Instance.UpdateText("Login/Text_ErrorInfo", "33333");
                    break;
                }
        }
    }
}
