# PNC-system-Chat-LLM-prototype

이 저장소는 Unity에서 동작하는 Chat-LLM (OpenAI) 연동 프로토타입 프로젝트입니다.

## 팀원 실행 가이드 (Getting Started)

Unity가 이미 설치되어 있는 팀원들은 다음 순서에 따라 프로젝트를 열고 실행할 수 있습니다.

### 1. 프로젝트 Clone
먼저 이 저장소를 로컬 PC로 클론(Clone) 받거나 다운로드합니다.
```bash
git clone https://github.com/Mr-BBAWWI/PNC-system-Chat-LLM-prototype.git
```

### 2. Unity 버전 확인
- 이 프로젝트는 **Unity 6000.3.11f1** 버전으로 생성되었습니다.
- Unity Hub에서 해당 버전을 설치해주세요. (버전이 다를 경우 호환성 문제가 생길 수 있습니다)

### 3. 프로젝트 열기
1. **Unity Hub**를 실행합니다.
2. `Projects` 탭에서 **[Add]** -> **[Add project from disk]** 를 클릭합니다.
3. 클론 받은 `PNC-system-Chat-LLM-prototype` 폴더를 선택하여 추가합니다.
4. 추가된 프로젝트를 클릭하여 Unity Editor로 엽니다.
> **참고**: 처음 프로젝트를 열 때 생략된 `Library` 등의 폴더를 다시 생성하므로 시간이 몇 분 정도 소요될 수 있습니다.

### 4. OpenAI API Key 설정 (필수!)
보안상 `.openai` 파일은 GitHub에 포함되지 않았습니다. API를 정상적으로 사용하려면 로컬 환경에 직접 파일(또는 환경변수 등 설정된 방식)을 생성해야 합니다.

1. 프로젝트 루트 폴더(`Assets` 폴더와 같은 위치)에 `.openai` 라는 이름의 텍스트 파일을 새로 만듭니다. (또는 기존에 설정하던 방식대로 키를 입력합니다)
2. 파일 열어서 여러분의 **OpenAI API Key**를 입력하고 저장합니다.

### 5. 실행 (Play)
- `Assets/Scenes/` 폴더 내에 있는 **DemoMapScene** 또는 **SampleScene**을 열고 에디터 상단의 Play [`▶`] 버튼을 눌러 테스트해봅니다.
