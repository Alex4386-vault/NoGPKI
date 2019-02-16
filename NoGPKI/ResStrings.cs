using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoGPKI
{
    public class ResStrings
    {
        internal string[] resStr = new string[26];
        public const int STR_ERR_MOUNT_FAIL = 0;
        public const int STR_ERR_NOT_AUTHORIZED = 1;
        public const int STR_MSG_PARTIAL_DISTRUST = 2;
        public const int STR_MSG_READY_FOR_CMD = 3;
        public const int STR_MSG_FULL_DISTRUST = 4;
        public const int STR_MSG_FULLLY_TRUSTED = 5;
        public const int STR_MSG_ABNORMALITY_FOUND = 6;
        public const int STR_ERR_MD5_CHECK_FAIL = 7;
        public const int STR_ERR_VALIDATION_FAIL = 8;
        public const int STR_MSG_CHECK_FILE_IS_THERE = 9;
        public const int STR_MSG_CHECK_TRUST = 10;
        public const int STR_MSG_CONFIRM_TRUST = 11;
        public const int STR_MSG_CHECKING_CHECKSUM = 12;
        public const int STR_ERR_CHECKSUM_FAIL = 13;
        public const int STR_MSG_PROCESS_TRUST = 14;
        public const int STR_MSG_TRUSTING_IN = 15;
        public const int STR_MSG_COMPLETED_TRUSTING = 16;
        public const int STR_ERR_WHILE_TRUST = 17;
        public const int STR_MSG_CANCELLED_TRUSTING = 18;
        public const int STR_MSG_CHECK_DISTRUST = 19;
        public const int STR_MSG_CONFIRM_DISTRUST = 20;
        public const int STR_MSG_PROCESS_DISTRUST = 21;
        public const int STR_MSG_DISTRUSTING_IN = 22;
        public const int STR_MSG_COMPLETED_DISTRUSTING = 23;
        public const int STR_ERR_WHILE_DISTRUST = 24;
        public const int STR_MSG_CANCELLED_DISTRUSTING = 25;

        private void initLang()
        {
            
        }

        public ResStrings()
        {
            initLang();
            setLanguage("kor");
        }

        // CultureInfo.ThreeLetterISOLanguage 를 이용한 시스템 UI 언어 판독
        // https://www.loc.gov/standards/iso639-2/php/code_list.php
        // https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.threeletterisolanguagename(v=vs.110).aspx
        public void setLanguage(string language)
        {
            // 잘라서 옵니다.
            switch (language)
            {
                default:
                case "eng":
                    //Do nothing. 영어로 둡니다.
                    break;
                case "kor":
                    replace(new Korean());
                    break;
            }
        }

        private void replace(ResStrings stringsSet)
        {
            if(stringsSet.resStr == null)
            {
                // language file must not be null;
                return;
            }
            for(int strCur = 0; strCur < resStr.Length; strCur++)
            {
                if(stringsSet.resStr[strCur] != null) { 
                    // put string only if it is not null;
                    resStr[strCur] = stringsSet.resStr[strCur];
                }
            }
        }
        
        public string getStr(int strNo)
        {
            return this.resStr[strNo];
        }
    }

    internal class Korean : ResStrings
    {
        private void initLang()
        {
            resStr[STR_ERR_MOUNT_FAIL] = "로컬 컴퓨터의 인증서 불신목록 RW 마운트 실패";
            resStr[STR_ERR_NOT_AUTHORIZED] = "오류: 권한 부족";
            resStr[STR_MSG_PARTIAL_DISTRUST] = "GPKI인증서를 불신, 일부만 적용됨";
            resStr[STR_MSG_READY_FOR_CMD] = "준비 완료! 명령만 주세요!";
            resStr[STR_MSG_FULL_DISTRUST] = "비밀키 일치 GPKI인증서 불신중";
            resStr[STR_MSG_FULLLY_TRUSTED] = "GPKI인증서를 신뢰하고 있음";
            resStr[STR_MSG_ABNORMALITY_FOUND] = "뭔가 이상해요! README를 읽어봐요!";
            resStr[STR_ERR_MD5_CHECK_FAIL] = "MD5 체크섬 검사 실패";
            resStr[STR_ERR_VALIDATION_FAIL] = "오류: 유효성 검사 실패";
            resStr[STR_MSG_CHECK_FILE_IS_THERE] = "다음 경로에 파일이 있는지 확인 해 주시기 바랍니다.\n있다면, 파일이 손상된것 같습니다.\n\n{0}";
            resStr[STR_MSG_CHECK_TRUST] = "GPKI 인증서 신뢰 확인";
            resStr[STR_MSG_CONFIRM_TRUST] = "정말로 인증서를 신뢰하시겠습니까?";
            resStr[STR_MSG_CHECKING_CHECKSUM] = "GPKI 인증서 체크섬 검사 중";
            resStr[STR_ERR_CHECKSUM_FAIL] = "GPKI 인증서 체크섬 검사 실패!";
            resStr[STR_MSG_PROCESS_TRUST] = "GPKI 인증서 신뢰 시작";
            resStr[STR_MSG_TRUSTING_IN] = "{0}에서 인증서 신뢰중";
            resStr[STR_MSG_COMPLETED_TRUSTING] = "GPKI 인증서 신뢰처리 완료";
            resStr[STR_ERR_WHILE_TRUST] = "GPKI 인증서 신뢰중 예외 발생! ({0})";
            resStr[STR_MSG_CANCELLED_TRUSTING] = "GPKI 인증서 신뢰 취소";
            resStr[STR_MSG_CHECK_DISTRUST] = "GPKI 인증서 불신 확인";
            resStr[STR_MSG_CONFIRM_DISTRUST] = "정말로 인증서를 불신하시겠습니까?";
            resStr[STR_MSG_PROCESS_DISTRUST] = "GPKI 인증서 불신 시작";
            resStr[STR_MSG_DISTRUSTING_IN] = "{0}에 인증서 불신중";
            resStr[STR_MSG_COMPLETED_DISTRUSTING] = "GPKI 인증서 불신 완료";
            resStr[STR_ERR_WHILE_DISTRUST] = "GPKI 인증서 불신중 예외 발생! ({0})";
            resStr[STR_MSG_CANCELLED_DISTRUSTING] = "GPKI 인증서 불신 취소";
        }
    }
}
