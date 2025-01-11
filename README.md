# FilePrepper

## Tasks
| 분류 | Task 이름 | 설명 |
|--------|------------|------|
| **파일 처리** | FileFormatConvert | 파일 형식 변환 |
| | SplitFile | 파일 분할 |
| | Merge | 파일 병합 |
| **컬럼 관리** | AddColumns | 새 컬럼 추가 |
| | RemoveColumns | 컬럼 제거 |
| | RenameColumns | 컬럼명 변경 |
| | ReorderColumns | 컬럼 순서 변경 |
| **데이터 정제** | DropDuplicates | 중복 제거 |
| | FillMissingValues | 결측치 처리 |
| | FilterRows | 행 필터링 |
| | ValueReplace | 값 대체/변환 |
| **데이터 변환** | DataTypeConvert | 데이터 타입 변환 |
| | ParseDate | 날짜 형식 파싱 |
| | DateExtraction | 날짜 컴포넌트 추출 |
| | OneHotEncoding | 원-핫 인코딩 |
| **데이터 스케일링** | NormalizeData | 데이터 정규화 |
| | ScaleData | 데이터 스케일링 |
| **데이터 분석** | Aggregate | 데이터 집계 |
| | DataSampling | 데이터 샘플링 |
| | BasicStatistics | 기초 통계량 계산 |
| | ColumnInteraction | 컬럼 상호작용 연산 |


## Dependency Packages
```
<PackageReference Include="CsvHelper" Version="33.0.1" />
<PackageReference Include="EPPlus" Version="7.5.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
<PackageReference Include="Scrutor" Version="5.1.1" />
```

