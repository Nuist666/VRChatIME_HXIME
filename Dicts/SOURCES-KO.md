# 일본어·한국어 사전 파일

다른 언어: [中文](SOURCES.md) ｜ [English](SOURCES-EN.md) ｜ [日本語](SOURCES-JP.md)

이 디렉터리에 추가한 사전은 기존 중국어 사전과 마찬가지로 UTF-8 `.txt` 텍스트이며, 형식은 「단어, 입력 코드, 가중치」를 실제 탭으로 구분한 것입니다. `PinyinDict` 의 「사전 불러와 적용」으로 적용한 뒤 「색인 다시 만들기」를 실행합니다. 프리팹의 `JapaneseDictionary` 와 `KoreanDictionary` 는 아래 표의 `japanese_mozc_common.dict.tsv.txt`, `korean_nikl_common.dict.tsv.txt` 에 연결되어 있습니다. 가중치는 생략할 수 있고(기본값 0) 음이 아닌 정수, RIME 에서 흔한 백분율(`luna_pinyin` 의 `99.93%` 등), 소수를 지원하며 백분율과 소수는 가져올 때 1/100 정밀도로 정수로 확대합니다.

| 파일 | 용도 |
| --- | --- |
| `japanese_mozc_common.dict.tsv.txt` | 일본어 간편 버전. 변환 가중치 상위 30,000 개 단어/코드 쌍. 먼저 이 버전으로 테스트하세요 |
| `japanese_mozc.dict.tsv.txt` | 일본어 전체 변환 결과. 더 많은 어형과 고유명사를 포함합니다. **경고: 단어 수가 너무 많아 직접 사용은 권장하지 않습니다.** |
| `korean_nikl_common.dict.tsv.txt` | 한국어 등급별 단어에서 가중치 상위 10,000 개 단어/코드 쌍. 먼저 이 버전으로 테스트하세요 |
| `korean_nikl.dict.tsv.txt` | 한국어 기초 사전 전체 변환 결과. 등급 없는 단어도 포함합니다. **경고: 단어 수가 너무 많아 직접 사용은 권장하지 않습니다.** |

「사전 불러와 적용」은 연결된 소스 파일을 분석해 단어와 가중치를 독립 사전 에셋에 기록하지만 색인은 만들지 않습니다. 이어서 같은 컴포넌트의 인스펙터에서 「색인 다시 만들기」를 실행하면 같은 에셋에 정렬된 색인이 기록됩니다. 두 단계는 메뉴에서도 실행할 수 있습니다: `Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Load and Apply Dictionary` 와 해당 `Rebuild Lookup Index`, 또는 `Tools → HXIME → Load and Apply All Dictionaries`, `Tools → HXIME → Rebuild All Lookup Indexes`. 분석 단계에서는 처리한 줄 수를 표시하고 Console 은 `[HXIME Dictionary Import]` 로 단계별 소요 시간을 기록합니다. 이 표시들은 전체 버전 사전 불러오기를 빠르게 해 주지 않습니다.

모든 사전은 `Assets/HXIME_DictionaryData/` 아래 독립된 바이너리 `.asset`(예: `SimpDictPool.asset`)으로 저장됩니다. 먼저 「사전 불러와 적용」이 단어를 기록하고, 다음으로 「색인 다시 만들기」가 같은 에셋에 정렬된 색인을 기록합니다. 프리팹과 씬은 사전 데이터나 색인 배열을 받지 않으며, 프리팹은 사전 소스 파일을 연결할 뿐입니다. 에셋은 사용자 프로젝트에서 생성되므로 패키지에 포함되지 않으며, 패키지에는 이 디렉터리의 소스 텍스트가 들어갑니다. 다시 불러오면 새 에셋이 만들어지므로 기존 에셋의 다른 참조에는 영향을 주지 않습니다. Play 모드 진입과 월드 빌드 시 에디터가 에셋의 단어와 색인을 Udon에 기록합니다. 불러오기나 색인이 없으면 Play 모드 진입이 취소되고 월드 빌드가 실패합니다. 큰 배열의 프리팹 오버라이드는 피할 수 있고, 어떤 언어의 불러오기와 색인 구성을 마친 뒤의 실행 시 검색 성능은 이전과 같습니다.

「전체」는 아래 필터 규칙을 거친 전체 결과를 뜻하며 원본 데이터의 모든 내용을 포함하지는 않습니다. 정확한 줄 수·바이트 수·SHA-256·업스트림 커밋 번호와 각 소스 파일 주소는 `dictionary-manifest.json` 에 있습니다. 간편 버전은 사람이 교정한 고빈도 단어 목록이 아닙니다. 현재 엔진은 정렬된 색인으로 조회하므로 대상 VRChat 플랫폼에서 지연을 계속 확인해야 합니다.

## 일본어 출처와 처리

- 출처: Google Mozc, `src/data/dictionary_oss/dictionary00.txt` ~ `dictionary09.txt`.
- 고정 리비전: [`13c98988247aa711d99db9e348ec2a597d14b5cd`](https://github.com/google/mozc/tree/13c98988247aa711d99db9e348ec2a597d14b5cd/src/data/dictionary_oss).
- 원본 행에서 읽기, 출력 단어, 단어 비용을 추출합니다. 코드는 pykakasi 2.3.0 의 Hepburn 출력으로 **원본 가나 읽기**에서 생성하며 한자에서 읽기를 추측하지 않습니다.
- 완전한 히라가나·가타카나·장음 부호로만 이루어진 읽기만 처리합니다. 숫자·기호가 있거나 라틴 문자 코드로 변환할 수 없는 읽기는 제외합니다. 코드 안의 `'` 와 `-` 는 허용합니다.
- 가중치는 `max(1, 40000 - 원본 비용)` 이며 원본 비용이 낮을수록 변환 가중치가 높습니다. 정렬용 근사값이고 실제 단어 빈도가 아니며 Mozc 의 품사 접속 비용이나 문장 분석 능력은 포함하지 않습니다.
- 「출력 단어, 코드」로 중복을 제거하고 최대 가중치를 남깁니다. 프로젝트 기존 일본어 예(가중치 50,000)를 추가합니다. 이어서 가중치 상위 30,000 쌍을 골라 간편 버전으로 만듭니다.
- 코드는 변환기의 장음·발음 등 규칙을 유지하지만 모든 입력기 별칭을 보장하지는 않습니다. 예: `nihongo` → `日本語`, `toukyou` → `東京`. 자동 변환 결과는 한 건씩 사람이 교정하지 않았습니다.

저작권은 Google LLC, NAIST 및 업스트림에 명시된 기타 권리자에게 있습니다. 업스트림 원문을 함께 포함합니다:

- [Mozc LICENSE](Licenses/mozc-LICENSE.txt): BSD 3-Clause.
- [사전 전용 고지](Licenses/mozc-dictionary-NOTICE.txt): IPAdic / NAIST / ICOT 조항과 Okinawa 데이터 고지를 포함합니다. 사전은 Mozc 코드 라이선스만으로 설명할 수 없으며 재배포 시 이 고지도 함께 유지하세요.

## 한국어 출처와 처리

- 원저자: 국립국어원(National Institute of Korean Language). 데이터셋: [한국어기초사전 / Korean Basic Dictionary](https://krdict.korean.go.kr/).
- 다운로드 출처: 커뮤니티가 관리하는 [`spellcheck-ko/korean-dict-nikl`](https://github.com/spellcheck-ko/korean-dict-nikl/tree/42c0d01889f34536e9cf94fe57f62bd2055b1bde/krdict), 고정 커밋 `42c0d01889f34536e9cf94fe57f62bd2055b1bde`, `krdict/001.xml` ~ `011.xml`. 이 미러는 국립국어원이 공식 관리하는 것이 아닙니다.
- 한국어 표제어, 텍스트 발음, 어휘 등급만 추출하며 원문의 예문·뜻풀이·음성 및 기타 미디어는 재배포하지 않습니다.
- 분석 전에 XML 1.0 이 허용하지 않는 제어 문자를 원본 XML 에서 제거하고 그 수를 매니페스트에 기록합니다. 내려받은 파일 자체는 수정하지 않습니다.
- 하이픈으로 시작하거나 끝나는 접사, 단독 자모, 순수 한국어가 아닌 표제어는 제외합니다. 표제어 내부의 하이픈 구분자는 제거하고 `^` 는 출력 공백으로 바꿉니다.
- korean-romanizer 0.28.0 으로 표제어에서 로마자 코드를 생성합니다. 처리 가능한 텍스트 발음이 있으면 발음 코드 별칭도 생성합니다. 코드는 공백을 제거하고 소문자로 만들며 출력 단어는 단어 사이 공백을 유지합니다. 자동 변환 결과는 한 건씩 사람이 교정하지 않았고 모든 로마자 표기 습관을 지원한다는 뜻이 아닙니다.
- 가중치: 초급 300, 중급 200, 고급 100, 등급 없음 10. 학습 등급 정렬이며 실제 단어 빈도가 아닙니다. 간편 버전은 등급별 단어와 예에서 가중치 상위 10,000 쌍을 고릅니다. 같은 가중치는 코드, 단어 순으로 정렬해 결과를 재현할 수 있게 합니다. 「단어, 코드」 중복은 최대 가중치를 남깁니다.
- 두 버전 모두 프로젝트 기존 한국어 예(가중치 1,000)를 추가하며 `hangugeo` → `한국어`, `annyeonghaseyo` → `안녕하세요` 같은 일반적인 입력을 포함합니다.

업스트림과 이 프로젝트가 변환한 한국어 사전은 **CC BY-SA 2.0 KR** 로 배포합니다. 이 프로젝트가 한 수정은 표제어 선별, 로마자 코드 추가, 정렬용 가중치 생성, 중복 제거, 예 추가입니다. 저작자, 데이터 출처, 이 수정 설명, [라이선스 링크](https://creativecommons.org/licenses/by-sa/2.0/kr/) 를 유지하고 파생 사전도 같은 라이선스를 유지하세요.

[미러 원문 설명](Licenses/nikl-README.md), [국립국어원 저작권 정책](https://krdict.korean.go.kr/kor/kboardPolicy/copyRightTermsInfo) 을 함께 포함합니다. 이 사전들의 라이선스는 프로젝트 코드 라이선스와 별개입니다.

## 출처 추적과 검증

이 디렉터리는 변환 결과와 라이선스만 패키지에 포함합니다. 빌드 스크립트는 `tools/build_dictionaries.py` 에 있으며, `dictionary-manifest.json` 에 기록된 주소에서 고정 리비전의 소스 데이터를 내려받아 바이트 수와 SHA-256 을 검증한 뒤 위 규칙으로 네 개의 사전을 다시 생성하고 매니페스트의 SHA-256 과 대조합니다. 따라서 변환은 저장소 안에서 재현할 수 있습니다. 소스 데이터는 계속 재배포하지 않으며 위 링크에서 직접 받아야 합니다. 실행 방법은 `python Dicts/tools/build_dictionaries.py` (재생성 후 검증)이며, `--check` 를 붙이면 고정 리비전 소스 데이터로 모든 사전을 다시 생성해 `dictionary-manifest.json` 의 SHA-256 과 바이트 단위로 대조하는 검증만 하고 이 디렉터리는 변경하지 않습니다. 변환 라이브러리는 `pykakasi==2.3.0`, `korean-romanizer==0.28.0` 으로 고정하며 내려받은 입력은 다시 실행할 때를 위해 캐시합니다(캐시 위치는 아래 `DEFAULT_CACHE`, 언제든 삭제 가능).

### 스크립트 설정 항목

가중치와 규모 설정은 `build_dictionaries.py` 상단에 모여 있으며, 값을 바꾸면 사전의 규모와 정렬 경향이 달라집니다:

| 상수 | 현재 값 | 역할 |
| --- | --- | --- |
| `JAPANESE_COST_BASE` | `40000` | 일본어 가중치 = 이 값 − Mozc 비용. 비용이 낮을수록 상위. 값을 올리면 일본어 전체 가중치가 올라갑니다 |
| `JAPANESE_SAMPLE_WEIGHT` | `50000` | 프로젝트 일본어 예에 주는 가중치. `japanese_sample.tsv.txt` 의 100/80/50 을 덮어씁니다 |
| `JAPANESE_COMPACT_SIZE` | `30000` | `japanese_mozc_common` 에 남기는 항목 수. 가중치 내림차순, 동률은 코드→단어 순으로 상위 N |
| `JAPANESE_ALLOWED` | 히라가나 `U+3041–U+3096`, 가타카나 `U+30A1–U+30FA`, `ー` `U+30FC` | 읽기에 허용되는 문자. 그 외는 가나가 아닌 것으로 줄 전체 제외 |
| `JAPANESE_CODE` | `^[a-z'-]+$` | 로마자 코드에 허용되는 문자. 맞지 않는 읽기는 버립니다 |
| `KOREAN_LEVEL_WEIGHT` | 초급 `300`, 중급 `200`, 고급 `100`, 등급 없음(`없음` 또는 미기재) `10` | 어휘 등급별 가중치 |
| `KOREAN_SAMPLE_WEIGHT` | `1000` | 프로젝트 한국어 예에 주는 가중치. `korean_sample.tsv.txt` 의 100 을 덮어씁니다 |
| `KOREAN_GRADED_MIN_WEIGHT` | `100` | `korean_nikl_common` 의 하한. 이 가중치 이상인 등급 항목만 상위 N 대상입니다 |
| `KOREAN_COMPACT_SIZE` | `10000` | `korean_nikl_common` 에 남기는 항목 수 |
| `KOREAN_WORD` | `^[가-힣]+( [가-힣]+)*$` | 표제어는 한글 음절과 단일 공백만. 하이픈 접사·단독 자모·비한국어·이중 공백은 제외 |

기타 설정: `DEFAULT_CACHE`(입력 캐시 위치, 기본값 `Temp/dict-cache`), `JAPANESE_SAMPLE` / `KOREAN_SAMPLE`(예 파일 경로), `HEADERS`(각 출력의 3줄 `#` 주석, SHA-256 에 포함). 상수를 바꾸면 출력이 달라지므로 스크립트를 다시 실행하고 `dictionary-manifest.json` 의 `outputs` 와 `statistics` 도 갱신해야 합니다.

공개 파일은 `dictionary-manifest.json` 으로 확인할 수 있습니다: `generator` 는 생성 스크립트(`Dicts/tools/build_dictionaries.py`), `outputs` 는 사전별 항목 수와 SHA-256, `bytes` 는 바이트 수, `sources` 와 `source_revisions` 는 업스트림 파일 주소와 커밋 번호, `conversion_packages` 는 당시 사용한 변환 라이브러리 버전, `statistics` 는 필터 통계를 기록합니다. 해시는 `#` 주석 줄을 포함한 파일 전체에 대해 SHA-256 으로 계산합니다.

변환에는 프로젝트의 소량 예만 보충으로 사용했고 그 검증값은 `local_supplements` 에 기록되어 있습니다. 라이선스와 저작자 표시 요구는 변환 방식이나 스크립트 위치에 따라 바뀌지 않으며 위 각 절이 계속 적용됩니다.

가져오기와 설정 절차는 [MULTILINGUAL-KO.md](../MULTILINGUAL-KO.md) 를 참고하세요. 같은 언어에서는 한 버전만 고르면 되고 전체 버전과 간편 버전을 동시에 불러올 필요는 없습니다.
