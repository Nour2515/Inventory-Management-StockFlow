export function toNumber(value: FormDataEntryValue | null) {
  return Number(value ?? 0);
}
