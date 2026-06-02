# Thiết kế Cơ sở Dữ liệu — Backend Chung cho Unity & Web Frontend

## 1. Tổng quan

Hệ thống sử dụng **PostgreSQL** làm cơ sở dữ liệu chính, với SQLAlchemy 2.0 (async) làm ORM trên nền FastAPI.  
Thiết kế đảm bảo:

- Dùng chung một backend cho cả **Unity Client** (AR học tập) và **Web Frontend** (dashboard phụ huynh).
- Hỗ trợ **JWT authentication** cho parent và child.
- Lưu trữ kết quả học tập đồng bộ từ Unity, hiển thị trên web dashboard.
- Quản lý nhiệm vụ, phần thưởng, cảm xúc, báo cáo.

---

## 2. Ký hiệu & Quy ước

- `PK` — Primary Key (UUID)
- `FK` — Foreign Key
- `UQ` — Unique Constraint
- `IX` — Index
- `DE` — Default Empty
- `?` — Nullable
- `CITEXT` — Case-insensitive text
- `TSVECTOR` — Full-text search vector
- Timestamps dùng `TIMESTAMPTZ` (UTC)

---

## 3. ER Diagram (Text)

```
Parents ──1:N── Children
Children ──1:N── ActivityResults
Children ──1:N── Sessions
Children ──1:N── SkillMastery
Children ──1:N── EmotionRecords
Children ──1:N── ChildTasks
Children ──1:N── RedemptionRequests
Children ──1:N── Inventory
Children ──1:N── Assessments
Children ──1:N── Reports
Children ──1:N── Interactions

Parents ──1:N── Notifications
Parents ──1:N── Tasks (TaskLibrary)

ChildTasks ──N:1── Tasks
Tasks ──1:N── ActivityRequirements (pivot)

Rewards ──1:N── RedemptionRequests
Rewards ──1:N── Inventory
Rewards ──1:N── AvatarItems

RefreshTokens ──N:1── Parents
RefreshTokens ──N:1── Children
```

---

## 4. Chi tiết các bảng

### 4.1. `parents` — Tài khoản phụ huynh

```sql
CREATE TABLE parents (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           CITEXT NOT NULL UNIQUE,
    phone           VARCHAR(20) UNIQUE,
    hashed_password TEXT NOT NULL,

    full_name       VARCHAR(255) NOT NULL,
    avatar_url      TEXT,
    role            VARCHAR(20) NOT NULL DEFAULT 'parent',

    notification_settings JSONB NOT NULL DEFAULT '{
        "progress_report": true,
        "emotion_alert": true,
        "task_reminder": true
    }'::jsonb,

    is_active       BOOLEAN NOT NULL DEFAULT true,
    is_verified     BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_parents_email ON parents (email);
```

### 4.2. `children` — Hồ sơ trẻ em

```sql
CREATE TYPE difficulty_level AS ENUM ('easy', 'medium', 'hard');

CREATE TABLE children (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID NOT NULL REFERENCES parents(id) ON DELETE CASCADE,

    username        VARCHAR(50) NOT NULL,
    hashed_password TEXT NOT NULL,
    display_name    VARCHAR(100) NOT NULL,
    age_years       SMALLINT NOT NULL CHECK (age_years >= 2 AND age_years <= 18),
    grade           VARCHAR(50),
    avatar_equipped UUID,          -- FK → avatar_items.id, set sau

    preferences     JSONB NOT NULL DEFAULT '{
        "volume": 0.8,
        "font_scale": 1.0,
        "animations": true
    }'::jsonb,

    is_active       BOOLEAN NOT NULL DEFAULT true,
    last_active_at  TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    UNIQUE (parent_id, username)
);

CREATE INDEX idx_children_parent ON children (parent_id);
```

### 4.3. `refresh_tokens` — Token làm mới JWT

```sql
CREATE TABLE refresh_tokens (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID REFERENCES parents(id) ON DELETE CASCADE,
    child_id        UUID REFERENCES children(id) ON DELETE CASCADE,
    token_hash      TEXT NOT NULL UNIQUE,
    expires_at      TIMESTAMPTZ NOT NULL,
    revoked         BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    -- Một trong hai parent_id hoặc child_id phải có giá trị
    CONSTRAINT chk_owner CHECK (
        (parent_id IS NOT NULL AND child_id IS NULL)
        OR (child_id IS NOT NULL AND parent_id IS NULL)
    )
);

CREATE INDEX idx_refresh_tokens_parent ON refresh_tokens (parent_id);
CREATE INDEX idx_refresh_tokens_child ON refresh_tokens (child_id);
```

### 4.4. `activities` — Danh mục hoạt động học tập

```sql
CREATE TABLE activities (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code            VARCHAR(50) NOT NULL UNIQUE,   -- 'quantity_match', 'compare_quantity', 'number_line_jump', 'number_bonds'
    title_vi        VARCHAR(255) NOT NULL,
    title_en        VARCHAR(255) NOT NULL,
    description_vi  TEXT,
    description_en  TEXT,
    icon_url        TEXT,
    sort_order      SMALLINT NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed data
-- code: quantity_match     → title_vi: 'Ghép Số Lượng'
-- code: compare_quantity   → title_vi: 'So Sánh Số Lượng'
-- code: number_line_jump   → title_vi: 'Nhảy Trên Trục Số'
-- code: number_bonds       → title_vi: 'Tách-gộp Số'
```

### 4.5. `lessons` — Bài học chi tiết

```sql
CREATE TABLE lessons (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    activity_id     UUID NOT NULL REFERENCES activities(id) ON DELETE CASCADE,
    code            VARCHAR(50) NOT NULL UNIQUE,   -- 'L01'..'L12'
    title_vi        VARCHAR(255) NOT NULL,
    title_en        VARCHAR(255) NOT NULL,
    difficulty      difficulty_level NOT NULL DEFAULT 'easy',
    skill_tags      TEXT[] NOT NULL DEFAULT '{}',
    prerequisites   UUID[] NOT NULL DEFAULT '{}',  -- Mảng lesson IDs
    sort_order      SMALLINT NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_lessons_activity ON lessons (activity_id);
CREATE INDEX idx_lessons_difficulty ON lessons (difficulty);
```

### 4.6. `sessions` — Phiên học (Unity gửi lên)

```sql
CREATE TABLE sessions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    activity_id     UUID NOT NULL REFERENCES activities(id),
    device_id       VARCHAR(255),
    start_time      TIMESTAMPTZ NOT NULL,
    end_time        TIMESTAMPTZ,
    duration_seconds INTEGER,           -- Tính toán từ start/end
    status          VARCHAR(20) NOT NULL DEFAULT 'in_progress'
                    CHECK (status IN ('in_progress', 'completed', 'abandoned')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    UNIQUE (id)    -- UUID đảm bảo uniqueness
);

CREATE INDEX idx_sessions_child ON sessions (child_id);
CREATE INDEX idx_sessions_child_activity ON sessions (child_id, activity_id);
CREATE INDEX idx_sessions_start ON sessions (child_id, start_time DESC);
```

### 4.7. `activity_results` — Kết quả từng câu/lượt chơi

```sql
CREATE TYPE error_type AS ENUM (
    'wrong_quantity', 'wrong_comparison', 'wrong_direction',
    'wrong_jump_count', 'timeout', 'other'
);

CREATE TABLE activity_results (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id            UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    session_id          UUID NOT NULL REFERENCES sessions(id) ON DELETE CASCADE,
    activity_id         UUID NOT NULL REFERENCES activities(id),
    lesson_id           UUID REFERENCES lessons(id),

    round_id            VARCHAR(100) NOT NULL,
    level_number        SMALLINT NOT NULL DEFAULT 1,
    difficulty          difficulty_level NOT NULL DEFAULT 'easy',

    is_correct          BOOLEAN NOT NULL,
    total_attempts      SMALLINT NOT NULL DEFAULT 1,
    hints_used_count    SMALLINT NOT NULL DEFAULT 0,
    time_spent_seconds  NUMERIC(8,2) NOT NULL DEFAULT 0,

    start_time          TIMESTAMPTZ NOT NULL,
    end_time            TIMESTAMPTZ NOT NULL,
    counts_toward_mastery BOOLEAN NOT NULL DEFAULT true,

    error_type          error_type,
    technical_issue     JSONB,      -- { "has_issue": bool, "issue_type": "...", "note": "..." }

    skill_tags          TEXT[] NOT NULL DEFAULT '{}',
    additional_data     JSONB,      -- Dữ liệu đặc thủ từng activity

    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    UNIQUE (session_id, round_id)
);

CREATE INDEX idx_results_child ON activity_results (child_id);
CREATE INDEX idx_results_session ON activity_results (session_id);
CREATE INDEX idx_results_lesson ON activity_results (lesson_id);
CREATE INDEX idx_results_skill ON activity_results USING GIN (skill_tags);
```

### 4.8. `skill_mastery` — Điểm thành thạo kỹ năng

```sql
CREATE TABLE skill_mastery (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    skill_tag       VARCHAR(100) NOT NULL,
    total_attempts  INTEGER NOT NULL DEFAULT 0,
    correct_attempts INTEGER NOT NULL DEFAULT 0,
    mastery_score   NUMERIC(5,2) NOT NULL DEFAULT 0
                    CHECK (mastery_score >= 0 AND mastery_score <= 100),
    last_practiced  TIMESTAMPTZ,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    UNIQUE (child_id, skill_tag)
);

CREATE INDEX idx_mastery_child ON skill_mastery (child_id);
```

### 4.9. `assessments` — Đánh giá đầu vào

```sql
CREATE TYPE assessment_type AS ENUM ('initial', 'periodic', 'custom');

CREATE TABLE assessments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    type            assessment_type NOT NULL DEFAULT 'initial',
    answers         JSONB NOT NULL,   -- Câu trả lời chi tiết
    summary         JSONB,            -- Tổng hợp điểm mạnh/yếu
    recommendations TEXT[],
    completed_at    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_assessments_child ON assessments (child_id);
```

### 4.10. `tasks` — Thư viện nhiệm vụ (do phụ huynh tạo)

```sql
CREATE TABLE tasks (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID NOT NULL REFERENCES parents(id) ON DELETE CASCADE,
    title           VARCHAR(255) NOT NULL,
    description     TEXT,
    difficulty      difficulty_level NOT NULL DEFAULT 'easy',
    category        VARCHAR(100),     -- 'quantity', 'comparison', 'number_line', 'number_bonds'
    estimated_minutes SMALLINT,
    is_public       BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_tasks_parent ON tasks (parent_id);
```

### 4.11. `activity_requirements` — Yêu cầu hoạt động cho nhiệm vụ

```sql
CREATE TABLE activity_requirements (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id         UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    activity_id     UUID NOT NULL REFERENCES activities(id),
    min_rounds      SMALLINT NOT NULL DEFAULT 1,
    target_score    NUMERIC(5,2),    -- Điểm tối thiểu (%)

    UNIQUE (task_id, activity_id)
);
```

### 4.12. `child_tasks` — Nhiệm vụ được giao cho trẻ

```sql
CREATE TYPE task_status AS ENUM (
    'assigned', 'in_progress', 'completed',
    'verified', 'rejected', 'given_up'
);

CREATE TABLE child_tasks (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    task_id         UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    assigned_by     UUID NOT NULL REFERENCES parents(id),

    status          task_status NOT NULL DEFAULT 'assigned',
    score           NUMERIC(5,2),     -- Điểm đạt được (%)
    started_at      TIMESTAMPTZ,
    completed_at    TIMESTAMPTZ,
    verified_at     TIMESTAMPTZ,
    verified_by     UUID REFERENCES parents(id),
    feedback        TEXT,
    given_up_reason TEXT,

    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    UNIQUE (child_id, task_id, status)  -- prevent duplicate active assignments
);

CREATE INDEX idx_child_tasks_child ON child_tasks (child_id);
CREATE INDEX idx_child_tasks_status ON child_tasks (child_id, status);
```

### 4.13. `rewards` — Cửa hàng phần thưởng

```sql
CREATE TYPE reward_category AS ENUM ('avatar', 'badge', 'accessory', 'special');

CREATE TABLE rewards (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID NOT NULL REFERENCES parents(id) ON DELETE CASCADE,
    name_vi         VARCHAR(255) NOT NULL,
    name_en         VARCHAR(255),
    description_vi  TEXT,
    description_en  TEXT,
    image_url       TEXT,
    category        reward_category NOT NULL DEFAULT 'badge',
    cost_points     INTEGER NOT NULL CHECK (cost_points > 0),
    quantity        INTEGER NOT NULL DEFAULT -1,  -- -1 = unlimited
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_rewards_parent ON rewards (parent_id);
```

### 4.14. `redemption_requests` — Yêu cầu đổi thưởng

```sql
CREATE TYPE redemption_status AS ENUM ('pending', 'approved', 'rejected');

CREATE TABLE redemption_requests (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    reward_id       UUID NOT NULL REFERENCES rewards(id) ON DELETE CASCADE,
    status          redemption_status NOT NULL DEFAULT 'pending',
    points_spent    INTEGER NOT NULL CHECK (points_spent > 0),
    reviewed_by     UUID REFERENCES parents(id),
    reviewed_at     TIMESTAMPTZ,
    reject_reason   TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_redemption_child ON redemption_requests (child_id);
CREATE INDEX idx_redemption_status ON redemption_requests (status);
```

### 4.15. `inventory` — Vật phẩm trẻ sở hữu

```sql
CREATE TABLE inventory (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    reward_id       UUID NOT NULL REFERENCES rewards(id) ON DELETE CASCADE,
    acquired_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_equipped     BOOLEAN NOT NULL DEFAULT false,

    UNIQUE (child_id, reward_id)
);

CREATE INDEX idx_inventory_child ON inventory (child_id);
```

### 4.16. `avatar_items` — Vật phẩm trang phục avatar

```sql
CREATE TYPE avatar_slot AS ENUM ('hat', 'outfit', 'accessory', 'pet', 'background');

CREATE TABLE avatar_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reward_id       UUID NOT NULL REFERENCES rewards(id) ON DELETE CASCADE,
    slot            avatar_slot NOT NULL,
    model_url       TEXT NOT NULL,
    thumbnail_url   TEXT,

    UNIQUE (reward_id)
);
```

### 4.17. `emotion_records` — Nhật ký cảm xúc

```sql
CREATE TYPE emotion_type AS ENUM (
    'happy', 'excited', 'neutral', 'confused',
    'frustrated', 'sad', 'bored', 'tired'
);

CREATE TABLE emotion_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    session_id      UUID REFERENCES sessions(id),
    emotion         emotion_type NOT NULL,
    intensity       SMALLINT CHECK (intensity >= 1 AND intensity <= 5),
    note            TEXT,            -- Ghi chú của phụ huynh
    source          VARCHAR(50) NOT NULL DEFAULT 'parent',  -- 'parent', 'child', 'ai'
    recorded_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_emotion_child ON emotion_records (child_id);
CREATE INDEX idx_emotion_time ON emotion_records (child_id, recorded_at DESC);
```

### 4.18. `interactions` — Tương tác phụ huynh-con

```sql
CREATE TABLE interactions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    parent_id       UUID NOT NULL REFERENCES parents(id),
    type            VARCHAR(50) NOT NULL DEFAULT 'chat',  -- 'chat', 'praise', 'coach'
    content         TEXT NOT NULL,
    metadata        JSONB,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_interactions_child ON interactions (child_id);
CREATE INDEX idx_interactions_time ON interactions (child_id, created_at DESC);
```

### 4.19. `reports` — Báo cáo phát triển

```sql
CREATE TYPE report_type AS ENUM ('weekly', 'monthly', 'custom', 'emotion');

CREATE TABLE reports (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    type            report_type NOT NULL DEFAULT 'monthly',
    title_vi        VARCHAR(255) NOT NULL,
    title_en        VARCHAR(255),
    summary         TEXT,
    data            JSONB NOT NULL,   -- Dữ liệu thống kê đầy đủ
    pdf_url         TEXT,
    period_start    DATE NOT NULL,
    period_end      DATE NOT NULL,
    generated_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_read         BOOLEAN NOT NULL DEFAULT false
);

CREATE INDEX idx_reports_child ON reports (child_id);
CREATE INDEX idx_reports_period ON reports (child_id, period_start DESC);
```

### 4.20. `notifications` — Thông báo

```sql
CREATE TYPE notification_category AS ENUM (
    'progress', 'emotion', 'task', 'report', 'reward', 'system'
);

CREATE TABLE notifications (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID NOT NULL REFERENCES parents(id) ON DELETE CASCADE,
    child_id        UUID REFERENCES children(id),
    category        notification_category NOT NULL,
    title_vi        VARCHAR(255) NOT NULL,
    title_en        VARCHAR(255),
    body_vi         TEXT NOT NULL,
    body_en         TEXT,
    data            JSONB,
    is_read         BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_notifications_parent ON notifications (parent_id, is_read, created_at DESC);
```

### 4.21. `points_ledger` — Sổ cái điểm thưởng

```sql
CREATE TYPE point_transaction_type AS ENUM (
    'earn_complete_task', 'earn_correct_answer', 'earn_streak',
    'spend_redeem', 'spend_giveup_penalty', 'admin_adjust'
);

CREATE TABLE points_ledger (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    child_id        UUID NOT NULL REFERENCES children(id) ON DELETE CASCADE,
    transaction_type point_transaction_type NOT NULL,
    amount          INTEGER NOT NULL,
    balance_after   INTEGER NOT NULL,
    reference_type  VARCHAR(50),   -- 'task', 'result', 'redemption'
    reference_id    UUID,
    description     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_points_child ON points_ledger (child_id);
CREATE INDEX idx_points_time ON points_ledger (child_id, created_at DESC);
```

### 4.22. `system_config` — Cấu hình hệ thống

```sql
CREATE TABLE system_config (
    key             VARCHAR(100) PRIMARY KEY,
    value           JSONB NOT NULL,
    description     TEXT,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed: points_per_correct_answer → 10
-- Seed: points_per_task_completed → 50
-- Seed: streak_bonus_multiplier → 1.5
```

---

## 5. Chú thích & Quy tắc Nghiệp vụ

| Quy tắc | Mô tả |
|---------|-------|
| **R1** | Mỗi `activity_result` thuộc về đúng một `session` và một `child`. |
| **R2** | `skill_mastery` được cập nhật sau mỗi `activity_result` mới: tăng `total_attempts`, tăng `correct_attempts` nếu `is_correct = true`, tính lại `mastery_score = (correct_attempts / total_attempts) * 100`. |
| **R3** | `mastery_score >= 80` coi là thành thạo (mastered). |
| **R4** | `points_ledger.balance_after` luôn >= 0. |
| **R5** | Một đứa trẻ không thể có hai `child_tasks` đang `assigned` cho cùng `task_id`. |
| **R6** | `redemption_requests` cần được phụ huynh duyệt trước khi vật phẩm vào `inventory`. |
| **R7** | `sessions` có `status = 'abandoned'` nếu kết thúc sớm (không hoàn thành mục tiêu). |
| **R8** | `refresh_tokens` bị thu hồi (revoke) khi người dùng đổi mật khẩu hoặc đăng xuất. |

---

## 6. Chỉ mục Full-text Search

```sql
-- Hỗ trợ tìm kiếm nhiệm vụ
ALTER TABLE tasks ADD COLUMN search_vector TSVECTOR
    GENERATED ALWAYS AS (
        to_tsvector('vietnamese', coalesce(title, '') || ' ' || coalesce(description, ''))
    ) STORED;

CREATE INDEX idx_tasks_search ON tasks USING GIN (search_vector);
```

---

## 7. Kế hoạch Migration & Seed

```sql
-- 1. activities (4 dòng)
-- 2. lessons (12 dòng)
-- 3. system_config (3 dòng)
-- 4. avatar_items (mẫu)
```

---

## 8. Entity-Relationship Summary

```
parents (1) ──────< (N) children
parents (1) ──────< (N) tasks
children (1) ──────< (N) activity_results
children (1) ──────< (N) sessions
children (1) ──────< (N) skill_mastery
children (1) ──────< (N) emotion_records
children (1) ──────< (N) child_tasks
children (1) ──────< (N) redemption_requests
children (1) ──────< (N) inventory
children (1) ──────< (N) assessments
children (1) ──────< (N) reports
children (1) ──────< (N) interactions
children (1) ──────< (N) points_ledger
tasks (1) ────────< (N) child_tasks
tasks (1) ────────< (N) activity_requirements
rewards (1) ───────< (N) redemption_requests
rewards (1) ───────< (N) inventory
rewards (1) ───────< (1) avatar_items
sessions (1) ──────< (N) activity_results
activities (1) ────< (N) lessons
activities (1) ────< (N) activity_requirements
activities (1) ────< (N) sessions
```

---

*Tài liệu này mô tả toàn bộ cấu trúc database cho hệ thống backend dùng chung. Bước tiếp theo: implement code theo thiết kế này.*
