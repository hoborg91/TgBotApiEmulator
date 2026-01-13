declare const __brand: unique symbol;
type Brand<T, B> = T & { [__brand]: B };

export type PositiveNumber = Brand<number, 'PositiveNumber'>;
export type EmailAddress = Brand<string, 'EmailAddress'>;

export function assertPositive(value: number): asserts value is PositiveNumber {
  if (value <= 0) {
    throw new Error(`Value must be positive, got [${value}].`);
  }
}

export function assertEmailAddress(value: string): asserts value is EmailAddress {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(value)) {
    throw new Error(`Invalid email format: [${value}].`);
  }
}

export function createPositiveNumber(value: number): PositiveNumber {
  assertPositive(value);
  return value as PositiveNumber;
}

export function createEmailAddress(value: string): EmailAddress {
  assertEmailAddress(value);
  return value as EmailAddress;
}
