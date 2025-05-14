#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>

#define MAX_LINE_LENGTH 1024
#define MAX_ID_LENGTH 256 // 語彙素IDの最大長を想定
#define MAX_UNIQUE_IDS 100001 // 想定されるユニークな語彙素IDの最大数

// 語彙素IDと頻度を格納する構造体
typedef struct {
    char id[MAX_ID_LENGTH];
    int frequency;
} IdFrequency;

IdFrequency id_frequencies[MAX_UNIQUE_IDS];
int unique_id_count = 0;

// 語彙素IDが既に登録されているか確認し、されていればそのインデックスを返す
int find_existing_id(const char* id) {
    for (int i = 0; i < unique_id_count; i++) {
        if (strcmp(id_frequencies[i].id, id) == 0) {
            return i;
        }
    }
    return -1; // 見つからなかった場合
}

int main(int argc, char *argv[]) {
    if (argc != 2) {
        fprintf(stderr, "Usage: %s <input_csv_file>\n", argv[0]);
        return 1;
    }

    FILE *fp = fopen(argv[1], "r");
    if (fp == NULL) {
        perror("Error opening CSV file");
        return 1;
    }

    char line[MAX_LINE_LENGTH];
    // CSVのヘッダー行を読み飛ばす
    fgets(line, MAX_LINE_LENGTH, fp);

    while (fgets(line, MAX_LINE_LENGTH, fp) != NULL) {
        char *token;
        char *rest = line;
        int column_count = 0;
        char current_id[MAX_ID_LENGTH];
        bool found_id = false;

        // カンマ区切りで各要素を解析
        while ((token = strtok_r(rest, ",", &rest)) != NULL) {
            column_count++;
            if (column_count == 7) {
                // 7番目の要素が語彙素ID
                strncpy(current_id, token, MAX_ID_LENGTH - 1);
                current_id[MAX_ID_LENGTH - 1] = '\0';
                found_id = true;
                break; // 語彙素IDが見つかったので、これ以上の解析は不要
            }
        }

        if (found_id) {
            // 既存のIDか確認
            int existing_index = find_existing_id(current_id);
            if (existing_index != -1) {
                // 既存のIDが見つかったら頻度をインクリメント
                id_frequencies[existing_index].frequency++;
            } else {
                // 新しいIDであれば登録
                if (unique_id_count < MAX_UNIQUE_IDS) {
                    strcpy(id_frequencies[unique_id_count].id, current_id);
                    id_frequencies[unique_id_count].frequency = 1;
                    unique_id_count++;
                } else {
                    fprintf(stderr, "Warning: Maximum number of unique IDs reached.\n");
                }
            }
        }
    }

    fclose(fp);

    // 結果の出力 (頻度順にソートする場合は、ここでソート処理を追加)
    printf("語彙素ID\t頻度\n");
    for (int i = 0; i < unique_id_count; i++) {
        printf("%s\t%d\n", id_frequencies[i].id, id_frequencies[i].frequency);
    }

    return 0;
}
