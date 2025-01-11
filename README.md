# FilePrepper

FilePrepper는 다양한 데이터 처리 작업을 수행할 수 있는 강력한 데이터 변환 및 분석 라이브러리입니다. 이 라이브러리는 컬럼 처리, 행 처리, 데이터 변환, 분석/집계, 원-핫 인코딩, 샘플링 등의 여러 작업을 지원하며, 사용자가 쉽게 설정 옵션을 구성하여 데이터 파이프라인을 구축할 수 있도록 도와줍니다.

## Dependency Packages
```
<PackageReference Include="CsvHelper" Version="33.0.1" />
<PackageReference Include="EPPlus" Version="7.5.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
<PackageReference Include="Scrutor" Version="5.1.1" />
```

## Tasks
| 구분       | 기능               | 설명                                     |
|------------|-------------------|----------------------------------------|
| 컬럼 처리  | AddColumns        | 새로운 컬럼을 추가하여 데이터 확장                  |
|            | RemoveColumns     | 불필요한 컬럼 제거                              |
|            | RenameColumns     | 컬럼 이름 변경                                 |
|            | ReorderColumns    | 컬럼 순서 변경                                 |
|            | ColumnInteraction | 컬럼 간 상호작용 계산 및 새로운 정보 추출            |
| 행 처리     | FilterRows        | 조건에 맞는 행 필터링                           |
|            | DropDuplicates    | 중복 행 제거                                  |
|            | FillMissingValues | 결측값 대체(평균, 중앙값, 지정 값 등)              |
| 변환        | DataTypeConvert   | 데이터 타입 변환                               |
|            | FileFormatConvert | 파일 형식 변환(CSV, JSON 등)                    |
|            | DateExtraction    | 날짜 데이터에서 연도, 월, 일 등 추출               |
|            | NormalizeData     | 데이터 정규화(스케일 조정)                         |
|            | ScaleData         | 데이터 스케일링(표준화, 최소-최대 변환 등)           |
|            | ValueReplace      | 특정 값 치환                                  |
| 분석/집계    | Aggregate         | 데이터를 그룹화하여 집계 통계 생성                  |
|            | BasicStatistics   | 기본 통계(평균, 분산 등) 계산                     |
|            | Merge             | 여러 데이터셋 병합                              |
| 변환/처리    | OneHotEncoding    | 범주형 데이터를 원-핫 인코딩로 변환                 |
| 샘플링       | DataSampling      | 데이터 샘플링으로 부분 집합 추출                   |

---

이하 각 작업별 옵션 세부사항을 참고하여 FilePrepper의 다양한 기능을 설정하고 활용할 수 있습니다.

### AddColumns
| 옵션       | 필수 | 설명                                       |
|------------|------|--------------------------------------------|
| NewColumns | 예   | 추가할 컬럼 이름과 기본값을 매핑하는 딕셔너리 |

### RemoveColumns
| 옵션          | 필수 | 설명             |
|---------------|------|------------------|
| RemoveColumns | 예   | 제거할 컬럼 목록 |

### RenameColumns
| 옵션      | 필수 | 설명                      |
|-----------|------|---------------------------|
| RenameMap | 예   | 기존 컬럼명을 새 이름으로 매핑 |

### ReorderColumns
| 옵션  | 필수 | 설명              |
|-------|------|-------------------|
| Order | 예   | 원하는 컬럼 순서 목록 |

### ColumnInteraction
| 옵션             | 필수   | 설명                                                                     |
|------------------|--------|--------------------------------------------------------------------------|
| SourceColumns    | 예     | 연산에 사용할 두 개 이상의 원본 컬럼 목록                                  |
| Operation        | 예     | 수행할 연산 타입 (Add, Subtract, Multiply, Divide, Concat, Custom)         |
| OutputColumn     | 예     | 결과를 저장할 출력 컬럼 이름                                              |
| CustomExpression | 조건부 | Operation이 Custom일 때 필요한 사용자 지정 연산식                        |

### FilterRows
| 옵션       | 필수 | 설명                                                   |
|------------|------|--------------------------------------------------------|
| Conditions | 예   | 필터 조건 목록. 각 조건은 ColumnName, Operator, Value 포함 |

### DropDuplicates
| 옵션              | 필수   | 설명                                                                      |
|-------------------|--------|---------------------------------------------------------------------------|
| KeepFirst         | 아니오 | 첫 번째 발견된 중복 데이터를 유지할지 여부 (기본값: true)                    |
| SubsetColumnsOnly | 아니오 | 특정 컬럼만 기준으로 중복 체크할지 여부 (기본값: false)                       |
| TargetColumns     | 조건부 | SubsetColumnsOnly true 시, 중복 체크에 사용할 컬럼 목록                      |

### FillMissingValues
| 옵션          | 필수 | 설명                                                                                         |
|---------------|------|----------------------------------------------------------------------------------------------|
| FillMethods   | 예   | 컬럼별 결측값 대체 방법 목록. 각 항목은 ColumnName, Method, FixedValue 포함                  |
| TargetColumns | 아니오 | 상속받은 대상 컬럼 목록. FillMethods에 의해 자동으로 업데이트됨                              |

### DataTypeConvert
| 옵션        | 필수 | 설명                                                                                                                         |
|-------------|------|------------------------------------------------------------------------------------------------------------------------------|
| Conversions | 예   | 변환할 컬럼 목록 및 설정. 각 항목은 ColumnName, TargetType, DateTimeFormat, DefaultValue, Culture, TrimWhitespace, IgnoreCase 포함 |

### FileFormatConvert
| 옵션            | 필수 | 설명                                                                                                   |
|-----------------|------|--------------------------------------------------------------------------------------------------------|
| TargetFormat    | 예   | 대상 파일 형식 (CSV, TSV, PSV, JSON, XML)                                                              |
| Encoding        | 아니오 | 파일 인코딩 설정                                                                                         |
| Delimiter       | 아니오 | 구분자. CSV/TSV/PSV 형식에서는 지정 불가                                                               |
| HasHeader       | 아니오 | 헤더 존재 여부 (기본값: true)                                                                            |
| DateTimeFormat  | 아니오 | 날짜/시간 형식                                                                                           |
| PrettyPrint     | 아니오 | JSON/XML 출력 시 가독성을 위한 들여쓰기 여부 (기본값: false)                                               |
| RootElementName | 아니오 | XML 출력 시 루트 엘리먼트 이름                                                                          |
| ItemElementName | 아니오 | XML 출력 시 아이템 엘리먼트 이름                                                                        |

### DateExtraction
| 옵션         | 필수 | 설명                                                                                                                 |
|--------------|------|----------------------------------------------------------------------------------------------------------------------|
| Extractions  | 예   | 날짜 추출 설정 목록. 각 항목은 SourceColumn, DateFormat, Culture, Components, OutputColumnTemplate 포함             |

### NormalizeData
| 옵션         | 필수 | 설명                                       |
|--------------|------|--------------------------------------------|
| TargetColumns| 예   | 정규화할 컬럼 목록                          |
| Method       | 예   | 정규화 방법 (MinMax 또는 ZScore)             |
| MinValue     | 조건부 | MinMax 사용 시 최소값                        |
| MaxValue     | 조건부 | MinMax 사용 시 최대값                        |

### ScaleData
| 옵션         | 필수 | 설명                                                                                 |
|--------------|------|--------------------------------------------------------------------------------------|
| ScaleColumns | 예   | 스케일링할 컬럼 및 방법 목록. 각 항목은 ColumnName, Method 포함                       |
| TargetColumns| 아니오 | 상속받은 대상 컬럼 목록. ScaleColumns에 의해 지정됨                                    |

### ValueReplace
| 옵션           | 필수 | 설명                                                                               |
|----------------|------|------------------------------------------------------------------------------------|
| ReplaceMethods | 예   | 치환 규칙 목록. 각 항목은 ColumnName, Replacements (치환 맵) 포함                     |
| TargetColumns  | 아니오 | 상속받은 대상 컬럼 목록. ReplaceMethods에 의해 지정됨                                 |

### Aggregate
| 옵션             | 필수 | 설명                                                                       |
|------------------|------|----------------------------------------------------------------------------|
| GroupByColumns   | 예   | 그룹화에 사용할 컬럼 목록                                                  |
| AggregateColumns | 예   | 집계 설정 목록. 각 항목은 ColumnName, Function, OutputColumnName 포함        |

### BasicStatistics
| 옵션          | 필수 | 설명                                      |
|---------------|------|-------------------------------------------|
| TargetColumns | 예   | 통계 계산 대상 컬럼 목록                    |
| Statistics    | 예   | 계산할 통계 유형 배열 (Mean, StdDev 등)   |
| Suffix        | 예   | 결과 컬럼명에 붙일 접미사                    |

### Merge
| 옵션           | 필수 | 설명                                                                              |
|----------------|------|-----------------------------------------------------------------------------------|
| InputPaths     | 예   | 병합할 파일 경로 목록 (최소 2개 이상 필요)                                         |
| MergeType      | 예   | 병합 유형 (Vertical 또는 Horizontal)                                             |
| JoinType       | 아니오 | Horizontal Merge 시 사용할 조인 유형 (Inner, Left, Right, Full)                    |
| JoinKeyColumns | 조건부 | Horizontal Merge 시, 조인 키로 사용할 컬럼 목록                                  |

### DataSampling
| 옵션               | 필수 | 설명                                                                                |
|--------------------|------|-------------------------------------------------------------------------------------|
| Method             | 예   | 샘플링 방법 (Random, Systematic, Stratified)                                         |
| SampleSize         | 예   | 샘플 크기 (비율 또는 절대값). 0보다 커야 함                                          |
| Seed               | 아니오 | 난수 생성을 위한 시드 값                                                              |
| StratifyColumn     | 조건부 | Stratified 방법 시 층화 기준 컬럼                                                     |
| SystematicInterval | 조건부 | Systematic 방법 시 샘플링 간격                                                        |

## FilePrepper CLI

FilePrepper CLI는 명령줄에서 FilePrepper의 모든 데이터 처리 기능을 사용할 수 있게 해주는 도구입니다.

### 설치

```bash
dotnet tool install --global FilePrepper.CLI
```

### 기본 사용법

모든 명령어는 다음과 같은 기본 구조를 따릅니다:

```bash
fileprepper <command> -i <input-file> -o <output-file> [options]
```

### 공통 옵션

모든 명령어에서 사용할 수 있는 공통 옵션:

| 옵션 | 필수 | 설명 |
|------|------|------|
| -i, --input | 예 | 입력 파일 경로 |
| -o, --output | 예 | 출력 파일 경로 |
| --ignore-errors | 아니오 | 처리 중 오류 발생 시 무시 (기본값: false) |
| --default-value | 아니오 | 오류 발생 시 사용할 기본값 |

### 명령어 목록

#### 컬럼 추가
```bash
# 새로운 컬럼 추가
fileprepper add-columns -i input.csv -o output.csv -c "NewCol1=Value1,NewCol2=Value2"
```

#### 컬럼 제거
```bash
# 특정 컬럼 제거
fileprepper remove-columns -i input.csv -o output.csv -c "Column1,Column2"
```

#### 컬럼 이름 변경
```bash
# 컬럼 이름 변경
fileprepper rename-columns -i input.csv -o output.csv -m "OldName1:NewName1,OldName2:NewName2"
```

#### 컬럼 순서 변경
```bash
# 컬럼 순서 재배치
fileprepper reorder-columns -i input.csv -o output.csv -o "Column1,Column2,Column3"
```

#### 컬럼 간 연산
```bash
# 컬럼 간 연산 수행
fileprepper column-interaction -i input.csv -o output.csv -s "Price,Quantity" -t Multiply -o "Total"

# 사용자 정의 연산식 사용
fileprepper column-interaction -i input.csv -o output.csv -s "Price,Quantity,Discount" -t Custom -o "FinalPrice" -e "($1 * $2) * (1 - $3)"
```

#### 행 필터링
```bash
# 조건에 따른 행 필터링
fileprepper filter-rows -i input.csv -o output.csv -c "Age:GreaterThan:30,Salary:LessThan:50000"
```

#### 중복 제거
```bash
# 모든 컬럼 기준 중복 제거
fileprepper drop-duplicates -i input.csv -o output.csv

# 특정 컬럼 기준 중복 제거
fileprepper drop-duplicates -i input.csv -o output.csv --subset-only -c "Name,Department"
```

#### 결측값 처리
```bash
# 결측값 대체
fileprepper fill-missing -i input.csv -o output.csv -m "Age:Mean,Salary:Median,Status:FixedValue:Unknown"
```

#### 데이터 타입 변환
```bash
# 데이터 타입 변환
fileprepper convert-type -i input.csv -o output.csv -c "Date:DateTime:yyyy-MM-dd,Age:Integer,Salary:Decimal"
```

#### 파일 형식 변환
```bash
# CSV를 JSON으로 변환
fileprepper convert-format -i input.csv -o output.json -t JSON --pretty

# CSV를 XML로 변환
fileprepper convert-format -i input.csv -o output.xml -t XML --root "Records" --item "Record"
```

#### 날짜 데이터 추출
```bash
# 날짜 컴포넌트 추출
fileprepper extract-date -i input.csv -o output.csv -e "OrderDate:Year,Month,Day:yyyy-MM-dd"
```

#### 데이터 정규화
```bash
# MinMax 정규화
fileprepper normalize -i input.csv -o output.csv -c "Salary,Age" -m MinMax --min 0 --max 1

# Z-Score 정규화
fileprepper normalize -i input.csv -o output.csv -c "Salary,Age" -m ZScore
```

#### 데이터 스케일링
```bash
# 데이터 스케일링
fileprepper scale -i input.csv -o output.csv -s "Price:MinMax,Score:Standardization"
```

#### 값 치환
```bash
# 값 치환
fileprepper replace -i input.csv -o output.csv -r "Status:active=1;inactive=0,Gender:M=Male;F=Female"
```

#### 데이터 집계
```bash
# 그룹별 집계
fileprepper aggregate -i input.csv -o output.csv -g "Department,Year" -a "Salary:Sum:TotalSalary,Age:Average:AvgAge"

# 집계 결과를 원본에 추가
fileprepper aggregate -i input.csv -o output.csv -g "Department" -a "Salary:Sum:TotalSalary" --append
```

#### 기본 통계 계산
```bash
# 통계 계산
fileprepper stats -i input.csv -o output.csv -c "Salary,Age" -s "Mean,StandardDeviation,Min,Max,Median"
```

#### 파일 병합
```bash
# 수직 병합 (행 추가)
fileprepper merge -i main.csv -o output.csv --inputs "data2.csv,data3.csv" -t Vertical

# 수평 병합 (조인)
fileprepper merge -i main.csv -o output.csv --inputs "data2.csv" -t Horizontal -j Left -k "ID,Department"
```

#### 원-핫 인코딩
```bash
# 범주형 변수 원-핫 인코딩
fileprepper one-hot-encoding -i input.csv -o output.csv -c "Category,Status" --drop-first
```

#### 데이터 샘플링
```bash
# 랜덤 샘플링
fileprepper data-sampling -i input.csv -o output.csv -m Random -s 0.3 --seed 42

# 층화 샘플링
fileprepper data-sampling -i input.csv -o output.csv -m Stratified -s 0.3 --stratify "Category"
```

### 에러 처리

- 모든 명령어는 성공 시 0, 실패 시 1을 반환합니다.
- 오류 발생 시 자세한 에러 메시지가 stderr에 출력됩니다.
- --ignore-errors 옵션을 사용하면 처리 중 발생하는 오류를 무시하고 계속 진행합니다.

### 팁과 주의사항

1. 파일 경로에 공백이 포함된 경우 따옴표로 묶어주세요:
```bash
fileprepper add-columns -i "input file.csv" -o "output file.csv" -c "NewCol=Value"
```

2. 여러 값을 지정할 때는 쉼표(,)로 구분합니다.

3. 복잡한 값이나 특수문자가 포함된 경우 적절히 이스케이프하거나 따옴표로 묶어주세요.

4. 대용량 파일 처리 시에는 충분한 메모리를 확보하세요.

5. 처리 전 항상 입력 파일을 백업하는 것을 권장합니다.