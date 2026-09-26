# 다국어 사전 입력

다른 언어: [中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md)

중국어 전체 병음, 약어 병음, 2벌식 병음과 간체／번체 사전은 그대로 유지됩니다. 추가한 언어는 각각 독립적인 「코드 → 단어」 사전을 사용합니다. 예: `nihongo` → `日本語`, `hangugeo` → `한국어`. 같은 방법으로 다른 언어도 추가할 수 있습니다. 프리팹 자체에는 단어와 색인이 들어 있지 않으므로, 사용할 언어마다 아래 두 단계를 직접 실행해야 합니다.

이것은 사전 기반 단어 단위 입력입니다. 전체 코드와 접두사 후보를 지원하지만, 임의의 로마자-가나 변환, 일본어 문장 해석, 한국어 2벌식 조합은 지원하지 않습니다. 코드는 가져온 사전과 일치해야 하며, 사전에 정의되어 있다면 편집란에 가나나 한글 코드를 직접 입력할 수도 있습니다.

## Unity에서 설정

`HXIME_Pinyin.prefab` 에는 단어와 색인이 전혀 들어 있지 않습니다. 프리팹의 네 사전 컴포넌트(`SimpDictPool`, `TradDictPool`, `JapaneseDictionary`, `KoreanDictionary`)는 모두 비어 있고, 각각 숨겨진 필드에 자신이 연결된 사전 소스 파일을 기록합니다: `SimpDictPool` → `Dicts/pinyin_simp.dict.yaml.txt`, `TradDictPool` → `Dicts/luna_pinyin.dict.yaml.txt`, `JapaneseDictionary` → `Dicts/japanese_mozc_common.dict.tsv.txt`, `KoreanDictionary` → `Dicts/korean_nikl_common.dict.tsv.txt`. 이 네 컴포넌트와 소스 파일은 이미 프리팹에 있으므로 새로 만들거나 다시 연결할 필요가 없습니다.

각 언어는 한 번에 하나씩 직접 설정하며, 해당 언어 컴포넌트의 `PinyinDict` 인스펙터에서 두 단계를 차례로 실행합니다: **「加载并应用字典 / Load and apply dictionary(사전 불러와 적용)」** 는 연결된 소스 파일을 분석해 단어와 가중치를 독립된 바이너리 사전 에셋 `Assets/HXIME_DictionaryData/<컴포넌트 오브젝트 이름>.asset`(예: `SimpDictPool.asset`)에 기록합니다. 이 단계는 색인을 만들지 않으며 프리팹과 씬을 변경하지도 않습니다. **「重建查询索引 / Rebuild lookup index(색인 다시 만들기)」** 는 같은 에셋에 정렬된 색인을 만듭니다. 단어와 색인은 이 바이너리 에셋에만 존재하며, 에셋은 사용자 프로젝트에서 생성되므로 패키지에 포함되지 않습니다.

에디터 메뉴도 언어별로 나뉘어 있습니다: `Tools → HXIME → Simplified Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`, `Tools → HXIME → Traditional Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`, `Tools → HXIME → Japanese → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`, `Tools → HXIME → Korean → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`, 그리고 `Tools → HXIME → Validate All Dictionaries`, `Tools → HXIME → Load and Apply All Dictionaries`, `Tools → HXIME → Rebuild All Lookup Indexes`. 예전 메뉴 `Tools → HXIME → Rebuild All Dictionary Indexes` 와 `Tools → HXIME → Configure Japanese and Korean Dictionaries` 는 더 이상 없습니다.

언어를 불러오지 않았거나 색인을 만들지 않았다면 해당 `PinyinDict` 인스펙터에 영어 ERROR 도움말 상자가 표시됩니다(예: `ERROR: HXIME Simplified Chinese dictionary data is not loaded. Click "Load and apply dictionary" in the PinyinDict inspector (source: Dicts/pinyin_simp.dict.yaml.txt).`, `ERROR: HXIME Simplified Chinese lookup index is not built. Click "Rebuild lookup index" in the PinyinDict inspector.`). Play 모드 진입은 취소되고(누락된 언어가 대화 상자에 표시됩니다) 월드 빌드는 즉시 실패합니다.

일본어와 한국어는 필요에 따라 켤 수 있습니다: `HXIMEUI` 최상단의 「是否启用日文?」「是否启用韩文?」(일본어 사용 / 한국어 사용) 토글이 해당 언어를 언어 순환에 포함할지 결정합니다. 끄면 언어 버튼이 그 언어를 건너뛰고 해당 사전도 조회하지 않습니다. 중국어와 영어는 항상 사용합니다. 사전 컴포넌트 자체는 삭제되지 않으므로 다시 켜려면 토글만 켜면 됩니다(`Tools → HXIME → Japanese → Enable Language Button` / `Tools → HXIME → Korean → Enable Language Button` 로도 전환할 수 있습니다).

간체·번체 컴포넌트는 그대로 `PinyinEngine` 의 `Dicts` 배열 앞 두 항목에, 일본어·한국어 컴포넌트는 `Additional Dicts` 배열에 연결되어 있으므로 배선을 직접 바꿀 필요가 없습니다. 그 밖에:

1. 함께 제공되는 TMP 글꼴은 정적 `NotoSansMultilingualFallback` 에 연결되어 있어 모든 현대 한글 음절과 기본 일·한 사전의 문자를 포함합니다. 출력 입력란이 프로젝트 외부 글꼴을 사용한다면 그 글꼴의 Fallback Font Assets 에 이 fallback 을 추가하세요. 사전을 추가한 뒤 새 문자가 글꼴에 포함되는지 확인하세요.
2. UdonSharp 를 컴파일하고 Unity Play Mode / VRChat ClientSim 에서 확인한 뒤 월드를 빌드합니다. 언어 버튼은 「중국어 → JP → Ko → 영어 → 중국어」를 순환하며 빈 참조는 건너뜁니다.

사전 소스 파일이 패키지에 포함되어 배포되고, 단어와 색인은 Unity 에디터에서 생성되어 월드와 함께 배포됩니다. VRChat 안에서 플레이어의 로컬 파일을 읽지 않습니다. 따라서 패키지에는 미리 적용된 사전 데이터 대신 사전 소스 텍스트(수십 MB)가 들어가며, 어떤 언어의 설정을 마친 뒤의 실행 시 검색 성능은 이전과 같습니다. 불러오기는 Undo 를 지원하고, 형식 오류가 있으면 이전 사전을 유지합니다.

`HXIMEUI.Target Inputfield` 는 반드시 별도의 출력란을 가리켜야 하며, `InputBarHandle/InputBar/InputField`(병음 편집란)를 가리키면 안 됩니다. 잘못 연결하면 확정된 텍스트가 병음 후보 재계산을 일으킵니다. 현재 버전은 이런 연결로는 확정을 거부합니다. 메뉴 **Tools → HXIME → Repair Output Field and Verify Chinese Selection** 은 현재 씬의 잘못된 연결을 복구합니다. 현재 씬을 백업하고, 후보 줄 아래에 별도 `OutputField` 를 만들어 다시 연결한 뒤 씬을 저장합니다. 이미 올바른 출력란은 교체하지 않습니다. 이 메뉴는 이어서 `HXIME_Pinyin.prefab` 을 메모리에서 열어 중국어 선택 자체 테스트(`ce s`, `ces`, `ce shi`, `c s`, 여분의 공백, 남은 코드)를 실행하고, 출력란이 편집란에 잘못 연결된 경우 거부되는지 확인합니다. 프리팹은 수정되지 않고 Play 모드로도 들어가지 않습니다.

메인 키보드와 상단 언어 라벨은 함께 갱신되며 현재 언어만 표시합니다: 중국어 `中`, 일본어 `JP`, 한국어 `Ko`, 영어 `En`. 지우기, 확정, 페이지 넘김, 설정, 입력 안내도 언어에 따라 바뀝니다. 영문 키는 여전히 사전의 로마자 코드를 입력합니다. 함께 제공되는 스킨의 작성자 설명은 원문을 유지합니다.

글꼴 출처와 라이선스는 `Fonts/MULTILINGUAL-FONT.md` 에 있습니다. 메뉴 **Tools → HXIME → Bake Japanese and Korean Font Fallback** 은 부족한 정적 글꼴을 생성하고 fallback 을 연결합니다. **Validate Multilingual Labels and Glyphs** 는 Play 모드에 들어가지 않고 라벨, TMP 메시 글리프, 한국어 `we` 후보를 검증합니다. 글꼴은 패키지에 미리 생성되어 있으므로 일반적인 가져오기에는 다시 굽지 않아도 됩니다. 이 도구들은 모두 편집 모드에서 실행되고 Play 모드로 들어가지 않으며 결과는 프로젝트의 에디터 임시 디렉터리(`Temp/`)에 기록됩니다. 수동 작업은 위 메뉴를 사용하세요.

## 파일 형식

UTF-8 텍스트이며 한 줄에 실제 탭으로 구분합니다: `단어<Tab>코드<Tab>가중치`. 가중치는 생략할 수 있고 기본값은 0입니다. 음이 아닌 정수, 백분율(`99.93%`), 소수(`1.5`)를 지원하며 뒤의 두 가지는 1/100 정밀도로 정수로 확대합니다(`99.93%` → `9993`, `1.5` → `150`). 이렇게 하면 원래 순서가 유지됩니다. 코드는 대소문자를 구분하지 않고 앞뒤 공백을 제거하며 내부 공백은 유지합니다. 빈 줄, `#` 로 시작하는 줄 주석, UTF-8 BOM, Windows 줄바꿈을 지원합니다.

표준 RIME `---` / `...` 헤더를 지원하며 데이터 열 순서는 text, code, weight 여야 합니다. 사용자 정의 `columns`, `import_tables` 를 쓰는 사전은 먼저 펼쳐서 위 TSV 로 변환해야 합니다. 다른 입력기의 인코딩 체계를 자동 변환하거나 외부 하위 사전을 불러오지는 않습니다.

확장 언어 후보는 「완전 코드 우선, 다음으로 가중치 내림차순」으로 정렬되고 같은 출력은 중복 제거됩니다. 상한은 기본 50개이며 Inspector에서 설정할 수 있습니다. 페이지당 5개입니다. 접두사 후보는 단어 전체를 완성하는 것이며, 선택하면 편집 문자열 전체를 지우고 출력 문자 수로 코드를 자르지 않습니다. 여러 단어로 된 표현도 별도 항목으로 가져와야 합니다.

`HXIMEUI` Inspector의 `Candidate Count` (`candidateLimits`)에서 전체 후보 상한을 설정할 수 있습니다. 기본값은 50, 최솟값은 1입니다. 페이지당 표시는 여전히 5개이며 실제 개수는 일치 결과에 따라 달라집니다. 100을 초과하면 Inspector에 영어 성능 경고가 표시되지만 값은 제한되지 않습니다. 상한을 높이면 검색, 중복 제거, 정렬 비용이 증가할 수 있습니다. 특히 짧은 입력이나 큰 사전에서는 VRChat 클라이언트에서 입력 지연을 확인하세요.

모든 사전은 `Assets/HXIME_DictionaryData/` 아래 독립된 바이너리 `.asset`(예: `SimpDictPool.asset`)으로 저장되며, 그 에셋에 단어와 만들어진 색인이 함께 들어갑니다. 프리팹과 씬은 사전 데이터나 색인 배열을 받지 않으며, 프리팹은 사전 소스 파일을 연결할 뿐입니다. 에셋은 사용자 프로젝트에서 생성되므로 패키지에 포함되지 않습니다. Play 모드 진입과 월드 빌드 시 에디터가 그 에셋의 단어와 색인을 Udon에 기록합니다. 불러오기나 색인이 없으면 Play 모드 진입이 취소되고 월드 빌드가 실패합니다. 큰 배열의 프리팹 오버라이드는 피할 수 있고, 어떤 언어의 불러오기와 색인 구성을 마친 뒤의 실행 시 검색 성능은 이전과 같습니다.

스페이스는 현재 페이지 첫 후보, 숫자 1–5는 현재 페이지 후보를 선택합니다. 후보가 없으면 스페이스로 원래 코드를 확정하고, Enter 로도 코드를 확정하며, Tab 은 편집 문자열을 지우고, 백스페이스는 코드 한 글자를 지웁니다. 언어를 전환하면 선택하지 않은 코드를 잃지 않도록 먼저 확정합니다. 중국어 전용 간체/번체, 2벌식 버튼은 중국어 모드에서만 표시됩니다.

## 확인 목록

- 언어는 「사전 불러와 적용」과 「색인 다시 만들기」를 모두 실행해야 사용할 수 있다. 둘 중 하나가 없으면 Play 모드 진입이 취소되고 월드 빌드도 실패한다. 중국어의 불러오기와 색인 구성이 끝나면 중국어, 영어, 간체/번체, 2벌식은 계속 동작한다.
- 일본어로 `NIHONGO` 가 `日本語`, `neko` 가 `猫` 와 `ねこ` 가 된다. 한국어로 `hangugeo` 가 `한국어` 가 된다.
- 접두사 입력, 일치 없음, 빈 문자열까지 삭제, 후보 페이지 넘김, 언어 전환 후에도 이전 후보나 빈 후보가 확정되지 않는다.
- 순서가 뒤섞인 항목, 중복 코드, 빈 파일, 음수 가중치, 종료 기호가 없는 RIME 헤더를 가져와 후보와 오류 메시지를 확인한다.
- 대상 플랫폼에서 글꼴 표시와 큰 사전의 입력 지연을 확인한다. 후보는 정렬 색인에 대한 이진 탐색으로 일치 구간을 찾고 항목을 하나씩 전부 검사하지 않습니다. 짧은 입력은 여전히 넓은 구간에 일치할 수 있으므로 매우 큰 사전의 프레임 시간은 실제 클라이언트에서 다시 확인해야 합니다. 색인은 상주 메모리를 더 쓰는 대신 검색을 빠르게 합니다. 자세한 내용은 [후보 검색 최적화](PERFORMANCE-KO.md).

## 관련 문서

- 사전 출처, 변환 규칙, 라이선스: [Dicts/SOURCES-KO.md](Dicts/SOURCES-KO.md)
- 글꼴 출처, 굽기 설정, 라이선스: [Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- 새 입력란에서 글자가 네모로 보이는 경우: [TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- 색인과 후보 검색 구현 설명: [PERFORMANCE-KO.md](PERFORMANCE-KO.md) ｜ [中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md)
- 설치와 자주 묻는 질문: [README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
