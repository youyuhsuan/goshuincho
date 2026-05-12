import { describe, it, expect, beforeEach } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import useLoadingStore from "../loading.store";

describe("useLoadingStore", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it("isGlobalLoading is false initially", () => {
    const store = useLoadingStore();
    expect(store.isGlobalLoading).toBe(false);
  });

  it("start() sets isGlobalLoading to true", () => {
    const store = useLoadingStore();
    store.start();
    expect(store.isGlobalLoading).toBe(true);
  });

  it("stop() after start() sets isGlobalLoading back to false", () => {
    const store = useLoadingStore();
    store.start();
    store.stop();
    expect(store.isGlobalLoading).toBe(false);
  });

  it("concurrent start() calls require matching stop() calls", () => {
    const store = useLoadingStore();
    store.start();
    store.start();
    store.stop();
    expect(store.isGlobalLoading).toBe(true);
    store.stop();
    expect(store.isGlobalLoading).toBe(false);
  });

  it("stop() when already idle does not go negative", () => {
    const store = useLoadingStore();
    store.stop();
    store.stop();
    expect(store.isGlobalLoading).toBe(false);
  });
});
