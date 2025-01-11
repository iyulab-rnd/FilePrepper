# FilePrepper

FilePrepper�� �پ��� ������ ó�� �۾��� ������ �� �ִ� ������ ������ ��ȯ �� �м� ���̺귯���Դϴ�. �� ���̺귯���� �÷� ó��, �� ó��, ������ ��ȯ, �м�/����, ��-�� ���ڵ�, ���ø� ���� ���� �۾��� �����ϸ�, ����ڰ� ���� ���� �ɼ��� �����Ͽ� ������ ������������ ������ �� �ֵ��� �����ݴϴ�.

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
| ����       | ���               | ����                                     |
|------------|-------------------|----------------------------------------|
| �÷� ó��  | AddColumns        | ���ο� �÷��� �߰��Ͽ� ������ Ȯ��                  |
|            | RemoveColumns     | ���ʿ��� �÷� ����                              |
|            | RenameColumns     | �÷� �̸� ����                                 |
|            | ReorderColumns    | �÷� ���� ����                                 |
|            | ColumnInteraction | �÷� �� ��ȣ�ۿ� ��� �� ���ο� ���� ����            |
| �� ó��     | FilterRows        | ���ǿ� �´� �� ���͸�                           |
|            | DropDuplicates    | �ߺ� �� ����                                  |
|            | FillMissingValues | ������ ��ü(���, �߾Ӱ�, ���� �� ��)              |
| ��ȯ        | DataTypeConvert   | ������ Ÿ�� ��ȯ                               |
|            | FileFormatConvert | ���� ���� ��ȯ(CSV, JSON ��)                    |
|            | DateExtraction    | ��¥ �����Ϳ��� ����, ��, �� �� ����               |
|            | NormalizeData     | ������ ����ȭ(������ ����)                         |
|            | ScaleData         | ������ �����ϸ�(ǥ��ȭ, �ּ�-�ִ� ��ȯ ��)           |
|            | ValueReplace      | Ư�� �� ġȯ                                  |
| �м�/����    | Aggregate         | �����͸� �׷�ȭ�Ͽ� ���� ��� ����                  |
|            | BasicStatistics   | �⺻ ���(���, �л� ��) ���                     |
|            | Merge             | ���� �����ͼ� ����                              |
| ��ȯ/ó��    | OneHotEncoding    | ������ �����͸� ��-�� ���ڵ��� ��ȯ                 |
| ���ø�       | DataSampling      | ������ ���ø����� �κ� ���� ����                   |

---

���� �� �۾��� �ɼ� ���λ����� �����Ͽ� FilePrepper�� �پ��� ����� �����ϰ� Ȱ���� �� �ֽ��ϴ�.

### AddColumns
| �ɼ�       | �ʼ� | ����                                       |
|------------|------|--------------------------------------------|
| NewColumns | ��   | �߰��� �÷� �̸��� �⺻���� �����ϴ� ��ųʸ� |

### RemoveColumns
| �ɼ�          | �ʼ� | ����             |
|---------------|------|------------------|
| RemoveColumns | ��   | ������ �÷� ��� |

### RenameColumns
| �ɼ�      | �ʼ� | ����                      |
|-----------|------|---------------------------|
| RenameMap | ��   | ���� �÷����� �� �̸����� ���� |

### ReorderColumns
| �ɼ�  | �ʼ� | ����              |
|-------|------|-------------------|
| Order | ��   | ���ϴ� �÷� ���� ��� |

### ColumnInteraction
| �ɼ�             | �ʼ�   | ����                                                                     |
|------------------|--------|--------------------------------------------------------------------------|
| SourceColumns    | ��     | ���꿡 ����� �� �� �̻��� ���� �÷� ���                                  |
| Operation        | ��     | ������ ���� Ÿ�� (Add, Subtract, Multiply, Divide, Concat, Custom)         |
| OutputColumn     | ��     | ����� ������ ��� �÷� �̸�                                              |
| CustomExpression | ���Ǻ� | Operation�� Custom�� �� �ʿ��� ����� ���� �����                        |

### FilterRows
| �ɼ�       | �ʼ� | ����                                                   |
|------------|------|--------------------------------------------------------|
| Conditions | ��   | ���� ���� ���. �� ������ ColumnName, Operator, Value ���� |

### DropDuplicates
| �ɼ�              | �ʼ�   | ����                                                                      |
|-------------------|--------|---------------------------------------------------------------------------|
| KeepFirst         | �ƴϿ� | ù ��° �߰ߵ� �ߺ� �����͸� �������� ���� (�⺻��: true)                    |
| SubsetColumnsOnly | �ƴϿ� | Ư�� �÷��� �������� �ߺ� üũ���� ���� (�⺻��: false)                       |
| TargetColumns     | ���Ǻ� | SubsetColumnsOnly true ��, �ߺ� üũ�� ����� �÷� ���                      |

### FillMissingValues
| �ɼ�          | �ʼ� | ����                                                                                         |
|---------------|------|----------------------------------------------------------------------------------------------|
| FillMethods   | ��   | �÷��� ������ ��ü ��� ���. �� �׸��� ColumnName, Method, FixedValue ����                  |
| TargetColumns | �ƴϿ� | ��ӹ��� ��� �÷� ���. FillMethods�� ���� �ڵ����� ������Ʈ��                              |

### DataTypeConvert
| �ɼ�        | �ʼ� | ����                                                                                                                         |
|-------------|------|------------------------------------------------------------------------------------------------------------------------------|
| Conversions | ��   | ��ȯ�� �÷� ��� �� ����. �� �׸��� ColumnName, TargetType, DateTimeFormat, DefaultValue, Culture, TrimWhitespace, IgnoreCase ���� |

### FileFormatConvert
| �ɼ�            | �ʼ� | ����                                                                                                   |
|-----------------|------|--------------------------------------------------------------------------------------------------------|
| TargetFormat    | ��   | ��� ���� ���� (CSV, TSV, PSV, JSON, XML)                                                              |
| Encoding        | �ƴϿ� | ���� ���ڵ� ����                                                                                         |
| Delimiter       | �ƴϿ� | ������. CSV/TSV/PSV ���Ŀ����� ���� �Ұ�                                                               |
| HasHeader       | �ƴϿ� | ��� ���� ���� (�⺻��: true)                                                                            |
| DateTimeFormat  | �ƴϿ� | ��¥/�ð� ����                                                                                           |
| PrettyPrint     | �ƴϿ� | JSON/XML ��� �� �������� ���� �鿩���� ���� (�⺻��: false)                                               |
| RootElementName | �ƴϿ� | XML ��� �� ��Ʈ ������Ʈ �̸�                                                                          |
| ItemElementName | �ƴϿ� | XML ��� �� ������ ������Ʈ �̸�                                                                        |

### DateExtraction
| �ɼ�         | �ʼ� | ����                                                                                                                 |
|--------------|------|----------------------------------------------------------------------------------------------------------------------|
| Extractions  | ��   | ��¥ ���� ���� ���. �� �׸��� SourceColumn, DateFormat, Culture, Components, OutputColumnTemplate ����             |

### NormalizeData
| �ɼ�         | �ʼ� | ����                                       |
|--------------|------|--------------------------------------------|
| TargetColumns| ��   | ����ȭ�� �÷� ���                          |
| Method       | ��   | ����ȭ ��� (MinMax �Ǵ� ZScore)             |
| MinValue     | ���Ǻ� | MinMax ��� �� �ּҰ�                        |
| MaxValue     | ���Ǻ� | MinMax ��� �� �ִ밪                        |

### ScaleData
| �ɼ�         | �ʼ� | ����                                                                                 |
|--------------|------|--------------------------------------------------------------------------------------|
| ScaleColumns | ��   | �����ϸ��� �÷� �� ��� ���. �� �׸��� ColumnName, Method ����                       |
| TargetColumns| �ƴϿ� | ��ӹ��� ��� �÷� ���. ScaleColumns�� ���� ������                                    |

### ValueReplace
| �ɼ�           | �ʼ� | ����                                                                               |
|----------------|------|------------------------------------------------------------------------------------|
| ReplaceMethods | ��   | ġȯ ��Ģ ���. �� �׸��� ColumnName, Replacements (ġȯ ��) ����                     |
| TargetColumns  | �ƴϿ� | ��ӹ��� ��� �÷� ���. ReplaceMethods�� ���� ������                                 |

### Aggregate
| �ɼ�             | �ʼ� | ����                                                                       |
|------------------|------|----------------------------------------------------------------------------|
| GroupByColumns   | ��   | �׷�ȭ�� ����� �÷� ���                                                  |
| AggregateColumns | ��   | ���� ���� ���. �� �׸��� ColumnName, Function, OutputColumnName ����        |

### BasicStatistics
| �ɼ�          | �ʼ� | ����                                      |
|---------------|------|-------------------------------------------|
| TargetColumns | ��   | ��� ��� ��� �÷� ���                    |
| Statistics    | ��   | ����� ��� ���� �迭 (Mean, StdDev ��)   |
| Suffix        | ��   | ��� �÷��� ���� ���̻�                    |

### Merge
| �ɼ�           | �ʼ� | ����                                                                              |
|----------------|------|-----------------------------------------------------------------------------------|
| InputPaths     | ��   | ������ ���� ��� ��� (�ּ� 2�� �̻� �ʿ�)                                         |
| MergeType      | ��   | ���� ���� (Vertical �Ǵ� Horizontal)                                             |
| JoinType       | �ƴϿ� | Horizontal Merge �� ����� ���� ���� (Inner, Left, Right, Full)                    |
| JoinKeyColumns | ���Ǻ� | Horizontal Merge ��, ���� Ű�� ����� �÷� ���                                  |

### DataSampling
| �ɼ�               | �ʼ� | ����                                                                                |
|--------------------|------|-------------------------------------------------------------------------------------|
| Method             | ��   | ���ø� ��� (Random, Systematic, Stratified)                                         |
| SampleSize         | ��   | ���� ũ�� (���� �Ǵ� ���밪). 0���� Ŀ�� ��                                          |
| Seed               | �ƴϿ� | ���� ������ ���� �õ� ��                                                              |
| StratifyColumn     | ���Ǻ� | Stratified ��� �� ��ȭ ���� �÷�                                                     |
| SystematicInterval | ���Ǻ� | Systematic ��� �� ���ø� ����                                                        |

## FilePrepper CLI

FilePrepper CLI�� ����ٿ��� FilePrepper�� ��� ������ ó�� ����� ����� �� �ְ� ���ִ� �����Դϴ�.

### ��ġ

```bash
dotnet tool install --global FilePrepper.CLI
```

### �⺻ ����

��� ��ɾ�� ������ ���� �⺻ ������ �����ϴ�:

```bash
fileprepper <command> -i <input-file> -o <output-file> [options]
```

### ���� �ɼ�

��� ��ɾ�� ����� �� �ִ� ���� �ɼ�:

| �ɼ� | �ʼ� | ���� |
|------|------|------|
| -i, --input | �� | �Է� ���� ��� |
| -o, --output | �� | ��� ���� ��� |
| --ignore-errors | �ƴϿ� | ó�� �� ���� �߻� �� ���� (�⺻��: false) |
| --default-value | �ƴϿ� | ���� �߻� �� ����� �⺻�� |

### ��ɾ� ���

#### �÷� �߰�
```bash
# ���ο� �÷� �߰�
fileprepper add-columns -i input.csv -o output.csv -c "NewCol1=Value1,NewCol2=Value2"
```

#### �÷� ����
```bash
# Ư�� �÷� ����
fileprepper remove-columns -i input.csv -o output.csv -c "Column1,Column2"
```

#### �÷� �̸� ����
```bash
# �÷� �̸� ����
fileprepper rename-columns -i input.csv -o output.csv -m "OldName1:NewName1,OldName2:NewName2"
```

#### �÷� ���� ����
```bash
# �÷� ���� ���ġ
fileprepper reorder-columns -i input.csv -o output.csv -o "Column1,Column2,Column3"
```

#### �÷� �� ����
```bash
# �÷� �� ���� ����
fileprepper column-interaction -i input.csv -o output.csv -s "Price,Quantity" -t Multiply -o "Total"

# ����� ���� ����� ���
fileprepper column-interaction -i input.csv -o output.csv -s "Price,Quantity,Discount" -t Custom -o "FinalPrice" -e "($1 * $2) * (1 - $3)"
```

#### �� ���͸�
```bash
# ���ǿ� ���� �� ���͸�
fileprepper filter-rows -i input.csv -o output.csv -c "Age:GreaterThan:30,Salary:LessThan:50000"
```

#### �ߺ� ����
```bash
# ��� �÷� ���� �ߺ� ����
fileprepper drop-duplicates -i input.csv -o output.csv

# Ư�� �÷� ���� �ߺ� ����
fileprepper drop-duplicates -i input.csv -o output.csv --subset-only -c "Name,Department"
```

#### ������ ó��
```bash
# ������ ��ü
fileprepper fill-missing -i input.csv -o output.csv -m "Age:Mean,Salary:Median,Status:FixedValue:Unknown"
```

#### ������ Ÿ�� ��ȯ
```bash
# ������ Ÿ�� ��ȯ
fileprepper convert-type -i input.csv -o output.csv -c "Date:DateTime:yyyy-MM-dd,Age:Integer,Salary:Decimal"
```

#### ���� ���� ��ȯ
```bash
# CSV�� JSON���� ��ȯ
fileprepper convert-format -i input.csv -o output.json -t JSON --pretty

# CSV�� XML�� ��ȯ
fileprepper convert-format -i input.csv -o output.xml -t XML --root "Records" --item "Record"
```

#### ��¥ ������ ����
```bash
# ��¥ ������Ʈ ����
fileprepper extract-date -i input.csv -o output.csv -e "OrderDate:Year,Month,Day:yyyy-MM-dd"
```

#### ������ ����ȭ
```bash
# MinMax ����ȭ
fileprepper normalize -i input.csv -o output.csv -c "Salary,Age" -m MinMax --min 0 --max 1

# Z-Score ����ȭ
fileprepper normalize -i input.csv -o output.csv -c "Salary,Age" -m ZScore
```

#### ������ �����ϸ�
```bash
# ������ �����ϸ�
fileprepper scale -i input.csv -o output.csv -s "Price:MinMax,Score:Standardization"
```

#### �� ġȯ
```bash
# �� ġȯ
fileprepper replace -i input.csv -o output.csv -r "Status:active=1;inactive=0,Gender:M=Male;F=Female"
```

#### ������ ����
```bash
# �׷캰 ����
fileprepper aggregate -i input.csv -o output.csv -g "Department,Year" -a "Salary:Sum:TotalSalary,Age:Average:AvgAge"

# ���� ����� ������ �߰�
fileprepper aggregate -i input.csv -o output.csv -g "Department" -a "Salary:Sum:TotalSalary" --append
```

#### �⺻ ��� ���
```bash
# ��� ���
fileprepper stats -i input.csv -o output.csv -c "Salary,Age" -s "Mean,StandardDeviation,Min,Max,Median"
```

#### ���� ����
```bash
# ���� ���� (�� �߰�)
fileprepper merge -i main.csv -o output.csv --inputs "data2.csv,data3.csv" -t Vertical

# ���� ���� (����)
fileprepper merge -i main.csv -o output.csv --inputs "data2.csv" -t Horizontal -j Left -k "ID,Department"
```

#### ��-�� ���ڵ�
```bash
# ������ ���� ��-�� ���ڵ�
fileprepper one-hot-encoding -i input.csv -o output.csv -c "Category,Status" --drop-first
```

#### ������ ���ø�
```bash
# ���� ���ø�
fileprepper data-sampling -i input.csv -o output.csv -m Random -s 0.3 --seed 42

# ��ȭ ���ø�
fileprepper data-sampling -i input.csv -o output.csv -m Stratified -s 0.3 --stratify "Category"
```

### ���� ó��

- ��� ��ɾ�� ���� �� 0, ���� �� 1�� ��ȯ�մϴ�.
- ���� �߻� �� �ڼ��� ���� �޽����� stderr�� ��µ˴ϴ�.
- --ignore-errors �ɼ��� ����ϸ� ó�� �� �߻��ϴ� ������ �����ϰ� ��� �����մϴ�.

### ���� ���ǻ���

1. ���� ��ο� ������ ���Ե� ��� ����ǥ�� �����ּ���:
```bash
fileprepper add-columns -i "input file.csv" -o "output file.csv" -c "NewCol=Value"
```

2. ���� ���� ������ ���� ��ǥ(,)�� �����մϴ�.

3. ������ ���̳� Ư�����ڰ� ���Ե� ��� ������ �̽��������ϰų� ����ǥ�� �����ּ���.

4. ��뷮 ���� ó�� �ÿ��� ����� �޸𸮸� Ȯ���ϼ���.

5. ó�� �� �׻� �Է� ������ ����ϴ� ���� �����մϴ�.