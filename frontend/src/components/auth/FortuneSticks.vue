<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const FORTUNE_CONFIGS = [
  { key: "daiKichi", color: "var(--p-primary-500)" },
  { key: "kichi",    color: "var(--p-primary-600)" },
  { key: "chuKichi", color: "var(--p-primary-700)" },
  { key: "shoKichi", color: "var(--p-primary-400)" },
  { key: "sueMichi", color: "var(--p-primary-800)" },
  { key: "kyo",      color: "var(--p-stone-600)"   },
] as const;

const LAYOUTS = [
  { left: 18,  top: 14,  rotate: -13 },
  { left: 88,  top: 3,   rotate:  8  },
  { left: 162, top: 18,  rotate: -7  },
  { left: 230, top: 5,   rotate: 16  },
  { left: 48,  top: 112, rotate: 12  },
  { left: 165, top: 100, rotate: -16 },
];

function shuffle<T>(arr: T[]): T[] {
  const a = [...arr];
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

interface Slip {
  id: number;
  key: string;
  color: string;
  left: number;
  top: number;
  rotate: number;
}

// Phase machine: hidden → dropping → settled
// Drives CSS class toggling so animation and hover don't conflict.
type Phase = "hidden" | "dropping" | "settled";

const slips     = ref<Slip[]>([]);
const selected  = ref<Slip | null>(null);
const flipped   = ref(false);
const hoveredId = ref<number | null>(null);
const phase     = ref<Phase>("hidden");
let settleTimer: ReturnType<typeof setTimeout> | null = null;

async function init() {
  if (settleTimer) clearTimeout(settleTimer);
  phase.value = "hidden";
  slips.value = shuffle([...FORTUNE_CONFIGS]).map((cfg, i) => ({
    id: i,
    key: cfg.key,
    color: cfg.color,
    ...LAYOUTS[i],
  }));

  // Let Vue render the hidden state before triggering the drop animation.
  await nextTick();
  phase.value = "dropping";

  // 1350 ms = last card delay (5×80 = 400ms) + animation (750ms) + 200ms buffer
  settleTimer = setTimeout(() => {
    phase.value = "settled";
  }, 1350);
}

onMounted(init);
onBeforeUnmount(() => {
  if (settleTimer) clearTimeout(settleTimer);
});

function draw(slip: Slip) {
  if (selected.value || phase.value !== "settled") return;
  selected.value = slip;
  setTimeout(() => {
    flipped.value = true;
  }, 450);
}

function reset() {
  flipped.value = false;
  setTimeout(() => {
    selected.value = null;
    void init();
  }, 700);
}
</script>

<template>
  <div class="select-none flex flex-col items-center gap-6">
    <p class="hint">{{ selected ? t("fortune.result") : t("fortune.pick") }}</p>

    <Transition name="stage" mode="out-in">
      <!-- Scattered cards -->
      <div v-if="!selected" key="scatter" class="scatter-stage">
        <div
          v-for="slip in slips"
          :key="slip.id"
          class="slip-wrap"
          :class="{
            'is-falling': phase === 'dropping',
            'is-settled': phase === 'settled',
            'is-hovered': phase === 'settled' && hoveredId === slip.id,
          }"
          :style="{
            left:    `${slip.left}px`,
            top:     `${slip.top}px`,
            '--rot': `${slip.rotate}deg`,
            '--delay': `${slip.id * 80}ms`,
            zIndex:  phase === 'settled' && hoveredId === slip.id ? 20 : slip.id + 1,
          }"
          @mouseenter="hoveredId = slip.id"
          @mouseleave="hoveredId = null"
          @click="draw(slip)"
        >
          <div class="omikuji-back">
            <span class="omikuji-back__glyph">御</span>
          </div>
        </div>
      </div>

      <!-- Revealed -->
      <div v-else key="reveal" class="reveal-stage">
        <div class="flip-scene">
          <div class="flip-card" :class="{ flipped }">
            <!-- Back face (large) -->
            <div class="flip-face flip-back">
              <div class="omikuji-back omikuji-back--lg">
                <span class="omikuji-back__glyph omikuji-back__glyph--lg">御</span>
              </div>
            </div>
            <!-- Front face -->
            <div class="flip-face flip-front">
              <div class="omikuji-front">
                <div class="omikuji-front__header">御神籤</div>
                <div class="omikuji-front__rule" />
                <div
                  class="omikuji-front__label"
                  :style="{ color: selected.color }"
                >
                  {{ t(`fortune.items.${selected.key}.label`) }}
                </div>
                <div class="omikuji-front__rule" />
                <p class="omikuji-front__message">
                  {{ t(`fortune.items.${selected.key}.message`) }}
                </p>
                <div class="omikuji-front__footer">◆</div>
              </div>
            </div>
          </div>
        </div>

        <Transition name="btn">
          <button v-if="flipped" class="again-btn" @click="reset">
            {{ t("fortune.drawAgain") }}
          </button>
        </Transition>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
/* ── Hint ───────────────────────────────────────── */
.hint {
  font-family: "Noto Serif TC", serif;
  font-size: 11px;
  letter-spacing: 0.28em;
  text-transform: uppercase;
  color: var(--p-stone-400);
}

/* ── Scatter stage ──────────────────────────────── */
.scatter-stage {
  position: relative;
  width: 308px;
  height: 220px;
}

/* ── Gravity phase machine ──────────────────────── */
@keyframes gravity-fall {
  0%   { opacity: 0; transform: rotate(var(--rot)) translateY(-300px); }
  62%  { opacity: 1; transform: rotate(var(--rot)) translateY(10px);   }
  76%  {             transform: rotate(var(--rot)) translateY(-5px);   }
  88%  {             transform: rotate(var(--rot)) translateY(2px);    }
  100% { opacity: 1; transform: rotate(var(--rot)) translateY(0);      }
}

/* Base: invisible, above viewport */
.slip-wrap {
  position: absolute;
  opacity: 0;
}

/* Phase: dropping — animation has full control of transform + opacity */
.slip-wrap.is-falling {
  animation: gravity-fall 0.75s cubic-bezier(0.25, 0.46, 0.45, 0.94)
    var(--delay, 0ms) both;
}

/* Phase: settled — remove animation, enable hover transition */
.slip-wrap.is-settled {
  animation: none;
  opacity: 1;
  transform: rotate(var(--rot)) translateY(0);
  transition: transform 0.3s cubic-bezier(0.34, 1.5, 0.64, 1);
  cursor: pointer;
}

/* Hover — declared AFTER .is-settled so it wins without !important */
.slip-wrap.is-hovered {
  transform: rotate(0deg) translateY(-18px) scale(1.08);
}

/* ── Omikuji paper back ─────────────────────────── */
.omikuji-back {
  width: 60px;
  height: 168px;
  border-radius: 3px;
  background: linear-gradient(180deg, #faf5e4 0%, #f0e8d2 100%);
  /* outer red border */
  outline: 1px solid rgba(185, 28, 28, 0.32);
  outline-offset: 0px;
  /* inner red border + depth shadow */
  box-shadow:
    inset 0 0 0 5px rgba(185, 28, 28, 0.00),
    inset 0 0 0 6px rgba(185, 28, 28, 0.13),
    2px 5px 16px rgba(0, 0, 0, 0.2),
    1px 2px 4px rgba(0, 0, 0, 0.1);
  /* horizontal grain lines */
  background-image:
    linear-gradient(180deg, #faf5e4 0%, #f0e8d2 100%),
    repeating-linear-gradient(
      180deg,
      transparent 0px,
      transparent 11px,
      rgba(140, 80, 40, 0.05) 11px,
      rgba(140, 80, 40, 0.05) 12px
    );
  background-blend-mode: multiply;
  display: flex;
  align-items: center;
  justify-content: center;
}

.omikuji-back--lg {
  width: 120px;
  height: 310px;
}

.omikuji-back__glyph {
  font-family: "Noto Serif TC", serif;
  font-size: 20px;
  font-weight: 300;
  color: rgba(185, 28, 28, 0.14);
  writing-mode: vertical-rl;
  user-select: none;
}

.omikuji-back__glyph--lg {
  font-size: 40px;
}

/* ── 3D flip ────────────────────────────────────── */
.reveal-stage {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 28px;
}

.flip-scene {
  perspective: 1000px;
  width: 120px;
  height: 310px;
}

.flip-card {
  width: 100%;
  height: 100%;
  position: relative;
  transform-style: preserve-3d;
  transition: transform 0.9s cubic-bezier(0.35, 0.18, 0.2, 1);
}

.flip-card.flipped {
  transform: rotateY(180deg);
}

.flip-face {
  position: absolute;
  inset: 0;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
}

.flip-front {
  transform: rotateY(180deg);
}

/* ── Omikuji front (revealed fortune) ──────────── */
.omikuji-front {
  width: 120px;
  height: 310px;
  border-radius: 3px;
  background: linear-gradient(180deg, #faf5e4 0%, #f0e8d2 100%);
  outline: 1px solid rgba(185, 28, 28, 0.32);
  box-shadow:
    inset 0 0 0 5px rgba(185, 28, 28, 0.00),
    inset 0 0 0 6px rgba(185, 28, 28, 0.13),
    3px 6px 22px rgba(0, 0, 0, 0.2);
  background-image:
    linear-gradient(180deg, #faf5e4 0%, #f0e8d2 100%),
    repeating-linear-gradient(
      180deg,
      transparent 0px,
      transparent 11px,
      rgba(140, 80, 40, 0.05) 11px,
      rgba(140, 80, 40, 0.05) 12px
    );
  background-blend-mode: multiply;
  display: flex;
  flex-direction: column;
  align-items: center;
  overflow: hidden;
}

.omikuji-front__header {
  width: 100%;
  background: var(--p-primary-700);
  color: #faf5e4;
  font-family: "Noto Serif TC", serif;
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.2em;
  writing-mode: vertical-rl;
  text-align: center;
  padding: 14px 0;
  flex-shrink: 0;
}

.omikuji-front__rule {
  width: 68%;
  height: 1px;
  background: rgba(185, 28, 28, 0.25);
  margin: 10px 0;
  flex-shrink: 0;
}

.omikuji-front__label {
  font-family: "Noto Serif TC", serif;
  font-size: 40px;
  font-weight: 700;
  line-height: 1;
  flex-shrink: 0;
}

.omikuji-front__message {
  font-family: "Noto Serif TC", serif;
  font-size: 9.5px;
  color: var(--p-stone-600);
  writing-mode: vertical-rl;
  line-height: 1.85;
  letter-spacing: 0.08em;
  flex: 1;
  overflow: hidden;
  margin: 0 4px;
}

.omikuji-front__footer {
  font-size: 7px;
  color: rgba(185, 28, 28, 0.22);
  padding: 8px 0;
  flex-shrink: 0;
}

/* ── Draw again button ──────────────────────────── */
.again-btn {
  font-family: "Noto Serif TC", serif;
  font-size: 11px;
  letter-spacing: 0.22em;
  background: transparent;
  border: 1.5px solid var(--p-primary-700);
  color: var(--p-primary-700);
  padding: 8px 26px;
  border-radius: 999px;
  cursor: pointer;
  opacity: 0.72;
  transition: opacity 0.2s;
}

.again-btn:hover {
  opacity: 1;
}

/* ── Transitions ────────────────────────────────── */
.stage-enter-active,
.stage-leave-active {
  transition: opacity 0.35s ease, transform 0.35s ease;
}
.stage-enter-from { opacity: 0; transform: translateY(10px); }
.stage-leave-to   { opacity: 0; transform: translateY(-10px); }

.btn-enter-active {
  transition: opacity 0.4s ease 0.4s, transform 0.4s ease 0.4s;
}
.btn-enter-from { opacity: 0; transform: translateY(8px); }
</style>
