#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define MAX_LINE_LEN 2048 // CSVの1行の最大長（必要に応じて調整）
#define EXPECTED_COLUMNS 7 // 期待される列の数

// 語彙素情報を格納する構造体
typedef struct {
    char *lexeme_id;    // 7番目の列: 語彙素ID
    char *word_form;    // 6番目の列: 語形(基本形)
    char *surface_form; // 3番目の列: 書字形（＝表層形）
    int frequency;
} LexemeInfo;

// 連結リストのノード
typedef struct Node {
    LexemeInfo data;
    struct Node *next;
} Node;

// 新しいノードを作成する関数
Node* create_node(const char *lexeme_id, const char *word_form, const char *surface_form) {
    Node *new_node = (Node*)malloc(sizeof(Node));
    if (!new_node) {
        perror("Failed to allocate memory for new node");
        exit(EXIT_FAILURE);
    }
    // strdupで各文字列を複製して格納
    new_node->data.lexeme_id = strdup(lexeme_id);
    new_node->data.word_form = strdup(word_form);
    new_node->data.surface_form = strdup(surface_form);

    if (!new_node->data.lexeme_id || !new_node->data.word_form || !new_node->data.surface_form) {
        perror("Failed to allocate memory for string data in new node");
        free(new_node->data.lexeme_id);
        free(new_node->data.word_form);
        free(new_node->data.surface_form);
        free(new_node);
        exit(EXIT_FAILURE);
    }
    new_node->data.frequency = 1;
    new_node->next = NULL;
    return new_node;
}

// 連結リストに語彙素情報を追加または更新する関数
void add_or_update_lexeme(Node **head, const char *lexeme_id, const char *word_form, const char *surface_form) {
    Node *current = *head;
    Node *prev = NULL;

    // 既存の語彙素IDを探す
    while (current != NULL) {
        if (strcmp(current->data.lexeme_id, lexeme_id) == 0) {
            current->data.frequency++;
            // 語形や書字形は最初に見つかったものを保持する
            return;
        }
        prev = current;
        current = current->next;
    }

    // 新しい語彙素IDの場合、新しいノードを作成してリストに追加
    Node *new_node = create_node(lexeme_id, word_form, surface_form);
    if (prev == NULL) { // リストが空、または先頭に追加
        *head = new_node;
    } else {
        prev->next = new_node;
    }
}

// 連結リストのメモリを解放する関数
void free_list(Node *head) {
    Node *current = head;
    while (current != NULL) {
        Node *next = current->next;
        free(current->data.lexeme_id);
        free(current->data.word_form);
        free(current->data.surface_form);
        free(current);
        current = next;
    }
}

// qsortのための比較関数（頻度の降順）
int compare_lexeme_info(const void *a, const void *b) {
    LexemeInfo *info_a = (LexemeInfo*)a;
    LexemeInfo *info_b = (LexemeInfo*)b;
    // 頻度で降順ソート
    if (info_b->frequency > info_a->frequency) return 1;
    if (info_b->frequency < info_a->frequency) return -1;
    // 頻度が同じ場合は語彙素IDで昇順ソート（任意、安定性のために）
    return strcmp(info_a->lexeme_id, info_b->lexeme_id);
}


int main(int argc, char *argv[]) {
    if (argc != 2) {
        fprintf(stderr, "使用法: %s <CSVファイルパス>\n", argv[0]);
        return EXIT_FAILURE;
    }

    FILE *file = fopen(argv[1], "r");
    if (!file) {
        perror("ファイルを開けませんでした");
        return EXIT_FAILURE;
    }

    char line[MAX_LINE_LEN];
    Node *lexeme_list_head = NULL;
    int line_number = 0;

    // ヘッダー行を読み飛ばす
    if (fgets(line, sizeof(line), file) == NULL && !feof(file)) {
        perror("ヘッダー行の読み込みエラーまたは空のファイルです");
        fclose(file);
        return EXIT_FAILURE;
    }
    line_number++;

    while (fgets(line, sizeof(line), file)) {
        line_number++;
        line[strcspn(line, "\r\n")] = 0; // 行末の改行文字を除去

        char *line_copy = strdup(line); 
        if (!line_copy) {
            perror("行データのメモリ確保に失敗しました");
            continue;
        }

        char *p = line_copy;
        int current_col_count = 0; // ループ前にリセット

        char* col_surface_form = NULL;
        char* col_word_form = NULL;
        char* col_lexeme_id = NULL;

        // 手動でのCSVパース
        while (p != NULL && *p != '\0' && current_col_count < EXPECTED_COLUMNS) {
            current_col_count++;
            char *token_start = p;
            char *next_comma = strchr(p, ',');

            if (next_comma != NULL) {
                *next_comma = '\0'; // トークンの終端
                p = next_comma + 1; // 次のトークンの開始位置へ
            } else {
                // 最後のトークン、または行内にこれ以上カンマがない
                p = NULL; // ループを終了させる
            }

            // token_start は現在のトークン（空文字列の場合もある）を指す
            if (current_col_count == 3) { col_surface_form = token_start; }
            else if (current_col_count == 6) { col_word_form = token_start; }
            else if (current_col_count == 7) { col_lexeme_id = token_start; }
        }
        // current_col_count は実際に処理した列の数

        // 警告条件の評価
        // 必要な列がすべて読み込めたか（current_col_count が7に達したか）
        // かつ、該当フィールドのポインタが（空文字列の場合でも）設定されたか。
        // 7列完全にパースできた場合、current_col_count は 7 になる。
        if (current_col_count == EXPECTED_COLUMNS && 
            col_surface_form != NULL &&  // 3列目が存在したか
            col_word_form != NULL &&     // 6列目が存在したか
            col_lexeme_id != NULL) {     // 7列目が存在したか
             add_or_update_lexeme(&lexeme_list_head, col_lexeme_id, col_word_form, col_surface_form);
        } else {
            fprintf(stderr, "警告: %d行目: 列数が不足しているか、必要なデータがありません (検出列数: %d): %s\n", 
                    line_number, current_col_count, line);
        }
        free(line_copy); // strdupで確保したメモリを解放
    }
    fclose(file);

    // 連結リストの要素数をカウント
    int num_unique_lexemes = 0;
    Node *current_node = lexeme_list_head;
    while (current_node != NULL) {
        num_unique_lexemes++;
        current_node = current_node->next;
    }

    if (num_unique_lexemes == 0) {
        printf("処理するデータがありませんでした。\n");
        free_list(lexeme_list_head);
        return EXIT_SUCCESS;
    }

    // 連結リストの内容を配列にコピーしてソート
    LexemeInfo *lexeme_array = (LexemeInfo*)malloc(num_unique_lexemes * sizeof(LexemeInfo));
    if (!lexeme_array) {
        perror("ソート用配列のメモリ確保に失敗しました");
        free_list(lexeme_list_head);
        return EXIT_FAILURE;
    }

    current_node = lexeme_list_head;
    for (int i = 0; i < num_unique_lexemes; i++) {
        lexeme_array[i] = current_node->data; // 構造体全体をコピー (ポインタもコピーされる)
        current_node = current_node->next;
    }

    qsort(lexeme_array, num_unique_lexemes, sizeof(LexemeInfo), compare_lexeme_info);

    // 結果を出力 (ヘッダーは出力しない)
    // printf("語彙素ID\t語形\t書字系\t頻度\n"); // 必要であればヘッダーを出力
    for (int i = 0; i < num_unique_lexemes; i++) {
        printf("%s\t%s\t%s\t%d\n",
               lexeme_array[i].lexeme_id,
               lexeme_array[i].word_form,
               lexeme_array[i].surface_form,
               lexeme_array[i].frequency);
    }

    // メモリ解放
    free(lexeme_array);             // ソート用に使った配列の解放
    free_list(lexeme_list_head);    // 連結リスト（と、その中の文字列データ）の解放

    return EXIT_SUCCESS;
}