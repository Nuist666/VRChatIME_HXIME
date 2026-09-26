# 중·일·한 후보 검색 최적화 안내

다른 언어: [中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md)

## 최적화 목표

키를 누를 때마다 발생하던 사전 전체 검색과 임시 할당을 줄여 입력 끊김을 완화합니다. 후보 상한은 기본 50개이며 Inspector에서 설정할 수 있습니다. 페이지당 5개입니다.

`HXIMEUI` Inspector의 `Candidate Count` (`candidateLimits`)에서 전체 후보 상한을 설정할 수 있습니다. 기본값은 50, 최솟값은 1입니다. 페이지당 표시는 여전히 5개이며 실제 개수는 일치 결과에 따라 달라집니다. 100을 초과하면 Inspector에 영어 성능 경고가 표시되지만 값은 제한되지 않습니다. 상한을 높이면 검색, 중복 제거, 정렬 비용이 증가할 수 있습니다. 특히 짧은 입력이나 큰 사전에서는 VRChat 클라이언트에서 입력 지연을 확인하세요.

## 주요 변경

- **검색 경로를 색인으로 통일**: 중국어, 일본어, 한국어가 같은 정렬 색인을 공유하고 이진 탐색으로 완전 일치와 접두사 일치 구간을 찾습니다.
- **언어 규칙 유지**: 중국어는 2벌식(双拼) 변환, 역방향 접두사, 간음(简拼) 일치를 유지하고, 일본어와 한국어는 로마자 접두사 일치와 출력 중복 제거를 유지합니다.
- **할당 감소**: 중국어는 Top-K을 스트리밍으로 처리하고 사전 크기의 후보 배열을 없앴습니다. 검색 작업 공간은 재사용하며 간음은 에디터에서 미리 계산합니다.
- **중복 계산 방지**: 최근 검색을 사전, 입력, 후보 상한, 일치 모드 기준으로 캐시하고 일치 처리의 핫 패스에서 로그 출력을 제거했습니다.
- **색인 사전 생성**: `PinyinDict` 인스펙터나 `Tools → HXIME` 메뉴에서 「색인 다시 만들기」를 실행하면 에디터가 정렬된 색인을 생성해 독립 바이너리 사전 에셋에 기록합니다. Play 모드 진입과 월드 빌드 시 에디터가 그 에셋의 단어와 색인을 Udon 데이터에 기록합니다.

## 사용 방법

「사전 불러와 적용」과 「색인 다시 만들기」를 실행하면 최적화가 적용됩니다. `PinyinDict` 인스펙터에서 **重建查询索引 / Rebuild lookup index** 를 누르거나 `Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Rebuild Lookup Index`, `Tools → HXIME → Rebuild All Lookup Indexes` 를 사용하세요(예전 `Tools → HXIME → Rebuild All Dictionary Indexes` 메뉴는 없습니다). 재구축은 독립 바이너리 사전 에셋(`Assets/HXIME_DictionaryData/`)만 갱신하며 프리팹과 씬은 변경하지 않으므로 에디터가 멈추지 않습니다. 개별 사전을 `PinyinDict` 인스펙터에서 다시 만들면 결과는 메뉴와 같지만 해당 컴포넌트만 처리합니다.

사용자 스크립트가 실행 중에 가중치를 바꾸면 `PinyinEngine.InvalidateMatchCache()` 를 호출해야 합니다. 코드와 항목 구조는 에디터에서 수정한 뒤 「사전 불러와 적용」과 「색인 다시 만들기」를 다시 실행해야 합니다. 데이터를 불러오지 않았거나 색인을 만들지 않은 언어는 Play 모드 진입을 취소하고 월드 빌드를 실패시킵니다. 해당 `PinyinDict` 인스펙터에는 영어 ERROR 도움말 상자가 표시됩니다.

## 검증과 제한

- Unity 2022.3.22f1 / UdonSharp 컴파일 통과.
- 개발 단계에서 1,197건의 후보 검사와 캐시 검사를 통과했으며 간체·번체 중국어, 일본어, 한국어, 동점 정렬, 중복 항목, 2벌식을 포함합니다.
- 네 개 사전의 색인이 Udon이 실제로 사용하는 데이터에 기록되는지 검증했습니다.
- 위 내용은 컴파일과 정확성 검증이며 VRChat 클라이언트의 프레임 시간 개선은 측정하지 않았습니다. 짧은 입력은 여전히 넓은 구간에 일치할 수 있습니다. 색인은 상주 메모리를 더 쓰는 대신 검색을 빠르게 합니다.

개발 단계의 임시 테스트 스크립트는 검증 완료 후 제거했습니다. 함께 제공되는 `Tools → HXIME → Rebuild All Lookup Indexes`, 각 언어의 `Rebuild Lookup Index` 와 `Validate Multilingual Labels and Glyphs` 메뉴는 영향을 받지 않고 그대로 사용할 수 있습니다.

## 관련 문서

- 다국어 사전 설정과 파일 형식: [MULTILINGUAL-KO.md](MULTILINGUAL-KO.md) ｜ [中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md)
- 사전 출처, 변환 규칙, 라이선스: [Dicts/SOURCES-KO.md](Dicts/SOURCES-KO.md)
- 설치와 자주 묻는 질문: [README-KO.md](README-KO.md) ｜ [中文](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md)
