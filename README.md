# NoGPKI
Distrusts GPKI Root CA Certificate because their security and certificate management is bad as F***  
정부의 행정전자서명 인증관리센터의 비정상적인 인증서관리 및 보안관리로 인해  
보안을 위해 행정전자서명의 루트 인증서의 대한 신뢰를 제거하는 프로그램입니다.  
(Microsoft Windows 운영체제는 정부의 탑재 요청으로 기본으로 신뢰함)  

## Table of Content
* [한국어](#한국어)
* [English](#English)  

## 한국어
### 왜 행정전자서명 인증서를 신뢰해서는 안되나요?

대표적으로 세가지 이유가 있습니다.  

1. 행정전자서명인증서의 구조자체가 안전하지 않습니다.
2. 부정발급된 인증서가 너무 많습니다.
3. 행정전자서명 인증서의 인증서 상태 프로토콜서버의 업타임이 저조합니다.

#### 행정전자서명인증서의 구조자체가 안전하지 않습니다.
모든 GPKI인증서 발급 및, EPKI 인증서 발급이 각각 하나의 subCA에 몰려 있습니다.  
이는 보안적으로 좋은 접근 방법이 아닙니다. 대부분의 인증서 발급 업체들은 Tree 구조로 만들어,  
하나의 subCA 에 문제가 발생 했을 시, rootCA에는 문제가 가지 않도록 설계합니다.    
해당 사항으로 인해 Mozilla사에서는 해당 구조상 보안 취약점으로 인해 [2015년 11월 GPKI인증서를 Firefox에 추가하지 않았습니다.](https://bugzilla.mozilla.org/show_bug.cgi?format=default&id=1226100)
앞에서 언급된 타 Root 인증서의 Tree 구조와는 다르게, 행정전자서명인증서의 경우 root CA에 subCA 를 할당하였기에, 해당 subCA에 문제가 생기면, GPKI 인증서 전체를 Revoke 해야 합니다.  
더 심각한 점은, 이 Revoke 마저 정상적으로 처리되지 않습니다. 이는 3번째 "인증서상태프로토콜서버의 업타임이 저조하다" 항목에서 다루겠습니다.

#### 부정발급된 인증서가 너무 많습니다.
관련된 [Chromium Bug Thread](https://bugs.chromium.org/p/chromium/issues/detail?id=823665)와 [Mozilla Bugzilla thread](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c32) 를 참고해 주시기 바랍니다.  
해당 버그 쓰레드에 따르면, GPKI 인증서는 다음에 대해 발행한 전적이 있습니다.  

* www.testssl.com (존재하는 도메인이나, 도메인 주인은 dadapro.com, 부정 발급.)
* www.ssltest.com (존재하는 도메인, 그러나 판매 중, 정부소유 아님, 부정 발급.)
* test_btms.seoul.go.kr (DNS Lookup 결과 아예 할당된 아이피 없음, 부정 발급.)
* test.*.pen.go.kr (중간에 들어간 와일드카드, 가이드라인 위반, 부정 발급.)
* test123.go.kr (DNS Lookup 결과 아예 할당된 아이피 없음, 부정 발급.)
* *.test.co.kr (yeoman57@naver.com이 주인인 도메인, 해당 서버에서는 https가 도입 되지 않음, 이 사람에게 발급한 것인지 의문)
* arasun.test.co.kr (위와 동일, 부정발급으로 추정)
* www.test1111.co.kr (DNS Lookup 결과 아예 할당된 아이피 없음, 부정 발급.)
* www.*.posan.ms.kr (중간에 들어간 와일드카드, 가이드라인 위반, 부정 발급.)
* e-csinfo.*.go.kr (중간에 들어간 와일드카드, 가이드라인 위반, 부정 발급.)
* 210.178.100.164 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* 211.206.120.182 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* 27.101.205.4 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* 61.108.124.4 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* 210.179.139.131 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* 27.101.119.206 (아이피 주소에 대한 인증서 발급, 가이드라인 위반, 부정 발급.)
* urk ([당연히] 존재하지도 않는 도메인, 이건 대체 뭐지..... 부정 발급)
* eais ([당연히] 존재하지도 않는 도메인, 이건 대체 뭐지..... 부정 발급)
* chs.cdc (존재하지도 않는 TLD, 부정 발급)
* go.kr (second-level TLD에 대한 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* [아이피주소에 할당된 추가 인증서](https://crt.sh/?icaid=123&dnsname=%25.___.___.%25)
* real ([당연히] 존재하지도 않는 도메인, 이건 대체 뭐지..... 부정 발급)
* cert.ust (존재하지도 않는 TLD, 부정 발급)
* ac.kr (second-level TLD에 대한 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.ssem.or.kr, *.serii.re.kr, *.kkulbaksa.com 에 대한 과도한 중복 인증서 발행  
  
아래의 내용은 민간의 개인 도메인 침범하거나, 기관의 인증서를 과도하게 부정 발급한 내용입니다.  
[인증서 목록](https://bugs.chromium.org/p/chromium/issues/detail?id=823665#c17)  
[crt.sh 216514419](https://crt.sh/?id=216514419)  
[crt.sh 169761218](https://crt.sh/?id=169761218)  
[crt.sh 140593669](https://crt.sh/?id=140593669)  
[crt.sh 107698017](https://crt.sh/?id=107698017)  
[crt.sh 93537384](https://crt.sh/?id=93537384)  
[crt.sh 61150414](https://crt.sh/?id=61150414)  
[crt.sh 61136851](https://crt.sh/?id=61136851)  
[crt.sh 20687119](https://crt.sh/?id=20687119)  

* *.sc.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.or.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.kg.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.hs.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.ms.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.es.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
* *.go.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)  
  
***co.kr에 대한 부정 발급 내용***  

[crt.sh 8169164](https://crt.sh/?id=8169164)  
[crt.sh 6990343](https://crt.sh/?id=6990343)  
[crt.sh 6797278](https://crt.sh/?id=6797278)  

* *.co.kr (second-level TLD에 대한 와일드카드 인증서 발급, 심각한 부정 발급, 루트CA 인증서 권한 남용)
  
2018년 3월 25일까지의 부정 발급에 대한 구글드라이브 스프레드 시트 데이터: [여기](https://docs.google.com/spreadsheets/d/1gsaZcvLY0vwe2humZn_3E3y89huqjPyS_syTHQCqy1Q/edit?usp=sharing)

추가적으로 확인된 부정 발급 리스트 [1](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c32) [2](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c33)  

#### 행정전자서명 인증서의 인증서 상태 프로토콜서버의 업타임이 저조합니다.
인증서가 한번 발급 된 후, 그 인증서가 revoke 되었는지 아닌지 확인 하기 위해 OCSP (Online Certificate Status Protocol)을 이용해 확인 합니다. 그러나, GPKI인증서는 이 OSCP 서버가 자주 offline 으로 유지됩니다.  
이게 정상적인 인증서만 GPKI가 발행 했다면 문제가 없지만, 정상적이지 않은 인증서도 GPKI 가 발행 하였기에 해당 인증서는 revoke 되지 못합니다. 이는 인증서의 신뢰성이 사라짐을 의미합니다.

### 그렇다면 어떻게 해야 할까요?
Linux, MacOS 그리고 Firefox (Windows)는 이미 GPKI에 대한 신뢰를 제거 하였습니다. 그러나 Microsoft 의 경우, 해당 Root 인증서에 대해 신뢰를 유지하고 있습니다.  
NoGPKI는, 여러분이 GPKI인증서를 손쉽게 불신하도록 도와 드립니다.  
1. 프로그램을 켭니다. (관리자 권한이 필요합니다)  
2. UAC화면 (이 프로그램이 컴퓨터를 변경하도록...) 화면에서 예를 누릅니다. (게시자를 알 수 없는 이유는 인증서가 없어서 그래요. 인증서 사주세요..... ㅠ)
3. 프로그램을 켜면 나오는 버튼 두개에서, 제거를 눌러 인증서를 불신, 복구를 눌러 다시 신뢰합니다 (뱅킹또는 민원24에서 해당 인증서 사용중)
4. 이제 여러분의 컴퓨터는 안전합니다.

### 지원
소스코드를 개선하셨다고요? 버그를 발견하셨다고요? PR과 리포트는 언제나 환영입니다! Issue로 남겨주세요!

### 라이선스
do What The Fuck you want to Public License (WTFPL) 로 배포되고 있습니다.  
~~이딴게 인증서냐 정부...~~

## English
### Reasons why you should NOT trust GPKI Root Certificates

1. The structure of GPKI Root Certificate is extremely vulnerable.
2. They have *literally* ***MASSIVE*** amount of misissued certificates.
3. Their OCSP servers are usually offline.
  
#### The structure of GPKI Root Certificate is extremely vulnerable.
GPKI Root Certificate is not using tree structure in their subCA system. which is highly vulnerable since one subCA issues almost all of the certificates.  
due to this faulty design, this will cause serious problem while revoking subCA-wide certificates.  
Plus, revoking will not work properly since they usually put their OCSP servers offline.

#### They have *literally* ***MASSIVE*** amount of misissued certificates
Please refer to these threads ( [Chromium Bug Thread](https://bugs.chromium.org/p/chromium/issues/detail?id=823665), [Mozilla Bugzilla thread](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c32) ) for more details.  
According to those bug threads, GPKI CARoot Certificate has been misissued certificate to these domains and IPs.

* www.testssl.com (dadapro.com holds the domain, Not a korean government-owned domain, Misissued.)
* www.ssltest.com (Existing domain, but for sale, and it is Not a korean government-owned domain, Misissued.)
* test_btms.seoul.go.kr (there is no any single record on this domain, misissued.)
* test.\*.pen.go.kr (wildcard in the middle, violation of CARoot Guidelines, Misissued)
* test123.go.kr (there is no any single record on this domain, misissued.)
* \*.test.co.kr (yeoman57@naver.com is owner, But server does not serve in https with *that* certificate, Seems to be misissued.)
* arasun.test.co.kr (Same with above.)
* www.test1111.co.kr (there is no any single record on this domain, misissued.)
* www.\*.posan.ms.kr (wildcard in the middle, violation of CARoot Guidelines, Misissued)
* e-csinfo.\*.go.kr (wildcard in the middle, violation of CARoot Guidelines, Misissued)
* 210.178.100.164 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* 211.206.120.182 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* 27.101.205.4 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* 61.108.124.4 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* 210.179.139.131 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* 27.101.119.206 (Certificate issued for IP Address, violation of CARoot Guidelines, Misissued)
* urk (Non-existent domain, Misissued)
* eais (Non-existent domain, Misissued)
* chs.cdc (Non-existent TLD, Misissued)
* go.kr (Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* [More certificates issued for IP Addresses](https://crt.sh/?icaid=123&dnsname=%25.___.___.%25)
* real (Non-existent domain, Misissued)
* cert.ust (Non-existent TLD, Misissued)
* ac.kr (Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* Too much certificate was issued for \*.ssem.or.kr, \*.serii.re.kr, \*.kkulbaksa.com   
  
These are the wildcard certificates at the second-level TLDs, which affects users of those TLDs.
[Certification List at chromium bugs](https://bugs.chromium.org/p/chromium/issues/detail?id=823665#c17)  
[crt.sh 216514419](https://crt.sh/?id=216514419)  
[crt.sh 169761218](https://crt.sh/?id=169761218)  
[crt.sh 140593669](https://crt.sh/?id=140593669)  
[crt.sh 107698017](https://crt.sh/?id=107698017)  
[crt.sh 93537384](https://crt.sh/?id=93537384)  
[crt.sh 61150414](https://crt.sh/?id=61150414)  
[crt.sh 61136851](https://crt.sh/?id=61136851)  
[crt.sh 20687119](https://crt.sh/?id=20687119)  

* \*.sc.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.or.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.kg.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.hs.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.ms.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.es.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
* \*.go.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)  
  
***Wildcard certificates issued against *.co.kr***  

[crt.sh 8169164](https://crt.sh/?id=8169164)  
[crt.sh 6990343](https://crt.sh/?id=6990343)  
[crt.sh 6797278](https://crt.sh/?id=6797278)  

* \*.co.kr (Wildcard Certificate issued to second-level TLD, Serious violation of CARoot Guidelines)
  
Spreadsheet data of misissued certificates until 2018-03-25: [여기](https://docs.google.com/spreadsheets/d/1gsaZcvLY0vwe2humZn_3E3y89huqjPyS_syTHQCqy1Q/edit?usp=sharing)

Another Lists of Misissued Certificates: [1](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c32) [2](https://bugzilla.mozilla.org/show_bug.cgi?id=1377389#c33)  

#### Their OCSP servers are usually offline.
In order to check certificates are valid or revoked, browsers usually accesses OCSP (Online Certificate Status Protocol) Server,  
But since GPKI puts their OCSP servers usually offline, their misissued certificates can not be revoked. This means their certificates can NOT be trusted.

### So What should I do?
Linux, MacOS and Firefox (Windows) already removed trust to GPKI Certificate, but Microsoft Windows Platform still trusts it.  
That's the reason NoGPKI kicked in, this program will help you to remove Korean GPKI Certificate easily.  

1. Execute the Application. (Requires an administrator privilege)  
2. Click Yes at the UAC Prompt (I don't have CodeSign Certificate, Sorry. ~~If you can, Please buy one for me ;(~~ )
3. Click [Delete] to distrust and click [Recover] to trust again (since minwon24 and south korean government still uses it)
4. Now, your computer is safe and great! Enjoy your internet!

### Contributions
Updated some code? Found some bug? PR and Bug reports are always welcome! Please leave an Issue about it!

### LICENSE
This program is distributed under do What The Fuck you want to Public License (WTFPL).  
~~Seriously, Korean Government. Do YOU REALLY THINK THAT IS A VALID ROOT CERTIFICATE?~~
