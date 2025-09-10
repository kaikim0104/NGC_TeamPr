using TMPro;
using UnityEngine;

public class BtnRenameListener : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputNickname;
    public void Rename()
    {
        string nickname = inputNickname.text;

        //닉네임 변경 시도
        int statusCode = Server.Instance.TryUpdateNickname(nickname);

        switch(statusCode)
        {
            //닉네임 변경 시도 성공 처리
            case 204:
                {
                    break;
                }
            //닉네임이 20자 이상일 경우 처리
            case 400:
                {
                    break;
                }
            //중복된 닉네임이 있을 경우 처리
            case 409:
                {
                    break;
                }
        }
    }
}
