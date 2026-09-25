### HXIME v0.9.5 VRChat 월드용 다국어 입력 키보드

다른 언어: [中文](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md)

VRChat 월드를 위한 입력 키보드입니다. 처음에는 중국어 병음 입력용이었지만, 지금은 **설정 가능한 다국어 입력**으로 확장되어 로마자 코드를 입력하면 일본어, 한국어 등 여러 언어의 단어를 바로 출력할 수 있습니다.

- **중국어**: 전체 병음, 약어 병음, 혼합 병음, 간체／번체 사전 전환
- **일본어**: 로마자 코드 입력. 예: `nihongo` → `日本語`, `neko` → `猫`／`ねこ`
- **한국어**: 로마자 코드 입력. 예: `hangugeo` → `한국어`, `annyeonghaseyo` → `안녕하세요`
- **영어**: 그대로 입력하며, 언제든 사전 입력과 전환할 수 있습니다
- 일본어와 한국어는 `HXIMEUI` 최상단에서 개별적으로 켜고 끌 수 있습니다. 중국어와 영어는 항상 사용합니다
- 사전은 Unity 에디터에서 가져와 월드와 함께 배포되며, 플레이어의 로컬 파일을 읽지 않습니다
- 설치가 간단하고 스킨을 바꿀 수 있습니다. 언어 버튼은 「中 → JP → Ko → En」을 순환합니다

`Dicts` 에는 바로 가져올 수 있는 Google Mozc 일본어 사전과 국립국어원 한국어기초사전이 있으며, 간편 버전과 전체 변환 버전을 모두 제공합니다. 파일 선택, 출처, 라이선스는 [사전 안내](Dicts/SOURCES.md)를 참고하세요.

현재 `HXIME_Pinyin.prefab` 은 일본어·한국어 간편 사전이 기본으로 연결되어 있어 바로 언어를 전환할 수 있습니다. 예전 씬에서는 `Tools → HXIME → Configure Japanese and Korean Dictionaries` 를 실행하세요. 자세한 설정 방법, 사전 파일 형식, 기능 범위는 [다국어 사전 입력](MULTILINGUAL-KO.md)에 정리했습니다([中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md)).

중국어·일본어·한국어는 같은 정렬 색인과 Top-30 후보 검색을 공유하고, 중국어는 역방향 접두사와 간음(简拼) 일치도 유지합니다. 색인은 사전 가져오기, Play 모드 진입, 월드 빌드 시 에디터에서 생성되므로 키를 누를 때는 관련 구간만 검색합니다. 예전 씬이나 직접 수정한 사전은 `Tools → HXIME → Rebuild All Dictionary Indexes` 로 다시 만들고 씬을 저장하세요. 사용자 스크립트가 실행 중에 사전 가중치를 바꾸면 엔진의 `InvalidateMatchCache()` 를 호출해야 합니다. 코드와 항목 구조는 에디터에서 수정한 뒤 색인을 다시 만들어야 합니다. 배경, 색인 구조, 검증 범위는 [후보 검색 최적화](PERFORMANCE-KO.md)에 정리했습니다([中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md)).

원작자는 아직 git을 잘 쓰지 못해서, PR을 보내주시는 모든 분께 감사드립니다.

“글자가 차원을 넘어, 문명이 이어지도록”

Project Link: https://github.com/xianglong90II/VRChatChineseIME_HXIME

# 설치 방법

- `HXIME_Pinyin` 프리팹을 맵에 드래그해서 넣습니다.
- `SwitchBarHandle`, `InputBarHandle`, `KeyboardHandle` 을 원하는 위치로 옮길 수 있습니다.
- 기본값은 첫 번째 스킨입니다. 스킨 순서를 바꾸면 기본 스킨을 바꿀 수 있습니다.
- (스킨 설명과 이미지는 1:1로 대응해야 합니다. 필요 없는 스킨은 삭제해도 됩니다.)
- `HXIME_Pinyin` 의 `HXIMEUI` 에서 Target Inputfield 에 출력할 입력란을 지정합니다. 입력기 자체의 병음 편집란이 아니라 **별도의 출력란**을 지정해야 합니다.
- 다국어 입력이 필요하면 [다국어 사전 입력](MULTILINGUAL-KO.md)에 따라 사전을 설정하세요. 현재 프리팹은 이미 설정되어 있습니다.
- 완료!

# Q&A

- Q: 입력기를 월드에 고정해서 플레이어가 집지 못하게 하려면?
- A: `SwitchBarHandle`, `InputBarHandle`, `KeyboardHandle` 의 VRC PickUp 을 삭제합니다.
- Q: 키보드 전환, 핸들 전환 같은 버튼을 만질 수 없게 하려면?
- A: `SwitchBar` 또는 `SettingsPanel` 아래 `Buttons` 에서 숨기려는 버튼을 선택하고 인스펙터 왼쪽 위의 체크를 해제합니다.
- Q: 어떤 언어를 지원하나요? 일본어 가나나 한국어를 입력할 수 있나요?
- A: 언어는 사전으로 결정됩니다. 중국어, 일본어, 한국어를 함께 제공하고 영어는 그대로 입력됩니다. 일본어와 한국어는 **단어 단위 코드 입력**입니다. 로마자 코드를 입력하고 후보를 고릅니다(예: `nihongo` → `日本語`). 임의의 로마자-가나 변환, 일본어 문장 해석, 한국어 2벌식 조합은 지원하지 않습니다. 가져온 사전에 정의되어 있다면 편집란에 가나나 한글을 직접 입력할 수도 있습니다. 사전을 추가하면 다른 언어도 입력할 수 있습니다.
- Q: 글자가 네모(□)로 보이면?
- A: [새 TMP 입력란에서 글자가 네모로 보이는 경우](TMP字体方框解决方案.md)와 `Fonts/MULTILINGUAL-FONT.md` 를 참고하세요.

# 고급

- Q: 직접 스킨을 만들 수 있나요?
- A: 물론입니다! 스킨 설명을 쓰고 배경 이미지를 준비하면 됩니다. 설명 형식은 「스킨 이름;설명;버튼 색 rgba;주 색상1 rgba;주 색상2 rgba」입니다.
- 예:
- HXIME白;作者: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- 정확하게 그리려면 `Themes` 폴더의 `ThemePSDInstruction.psd` 와 png 파일을 참고하세요.
- 배경에는 UISprite 대신 Texture2D 를 사용합니다.
- 온라인 로딩이나 RenderTexture(게임 내 카메라 촬영)도 시도할 수 있습니다.
- Q: 스킨은 몇 세트까지 넣을 수 있나요?
- A: 이론상 거의 무제한입니다. 다만 플레이어가 선택할 수 있는 슬롯은 처음 9개뿐입니다. 10번째 이후는 `HXIMEUI` 가 있는 오브젝트에서 공개 메서드 `SetSkin(index)` 를 호출해 설정합니다.
- Q: 직접 만든 사전을 가져오려면?
- A: 중국어 사전은 `PinyinEngine` 의 간체·번체 슬롯에, 다른 언어는 `Additional Dicts` 배열에 있습니다. 배열의 각 항목은 `PinyinDict` 컴포넌트가 있는 자식 오브젝트이며, 「언어 버튼 이름」이 버튼에 표시되는 문자입니다.
- 「파일 찾아보기」와 「사전 불러와 적용」으로 UTF-8 텍스트 사전을 가져옵니다. 형식은 「단어 tab 코드 tab 가중치(생략 가능, 기본 0)」입니다. 확장 언어의 코드는 로마자이며 대소문자를 구분하지 않습니다.
- 눈치채셨겠지만 RIME 사전 형식과 같습니다. `---` / `...` 헤더가 있는 표준 RIME 사전도 바로 가져올 수 있고, 사용자 정의 `columns` / `import_tables` 가 있는 사전은 먼저 위 세 열로 펼쳐야 합니다. 자세한 내용은 [다국어 사전 입력](MULTILINGUAL-KO.md).
- Q: 확장 언어의 키 조작은?
- A: 스페이스는 현재 페이지 첫 후보, 숫자 1–5는 현재 페이지 후보 선택입니다. 후보가 없으면 스페이스나 Enter 로 코드를 그대로 확정하고, Tab 은 편집 문자열을 지우고, 백스페이스는 코드를 한 글자 지웁니다. 언어를 바꾸면 선택하지 않은 코드가 먼저 확정됩니다.

# Licence: LGPL v3
https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Credit
- 아이콘 Icons: Google Material UI & Fonts https://fonts.google.com/
- 글꼴 Font: Noto Sans / Noto Sans CJK https://fonts.google.com/noto
- 간체 사전: RIME pinyin-simp https://github.com/rime/rime-pinyin-simp
- 번체 사전: RIME luna-pinyin https://github.com/rime/rime-luna-pinyin
- 일본어 사전: Google Mozc https://github.com/google/mozc (BSD 3-Clause, 사전에 별도 고지 포함)
- 한국어 사전: 국립국어원 한국어기초사전 https://krdict.korean.go.kr/ (CC BY-SA 2.0 KR)
- 출처, 변환 규칙, 라이선스 전문: [Dicts/SOURCES.md](Dicts/SOURCES.md)
