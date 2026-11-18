export type ControlType =
  | 'switch'
  | 'select'
  | 'bitmask'
  | 'text'
  | 'textarea'
  | 'number'
  | 'color'
  | 'image'

export interface SettingOption {
  value: string
  label: string
  /** Shown on the legacy screen but the bit does not fit the column type. */
  unsupported?: boolean
}

export interface SettingDependency {
  key: string
  in?: string[]
  notIn?: string[]
  message: string
}

export interface SettingDefinition {
  key: string
  group: string
  label: string
  control: ControlType
  sqlType: string
  maxLength: number
  bitwise: boolean
  options?: SettingOption[]
  description?: string
  legacyControlId?: string
  dependsOn?: SettingDependency
  placeholder?: string
  min?: number
  knownMask?: string
}

export interface SettingGroup {
  key: string
  title: string
  description: string
}

export interface SettingsCatalog {
  version: number
  source: string
  groups: SettingGroup[]
  settings: SettingDefinition[]
  notInDatabase: { legacyControlId: string; label: string }[]
}

/** Every value is a string (or null): bigint masks do not fit in JS numbers. */
export type SettingValues = Record<string, string | null>

export interface SettingsResponse {
  row: number
  rowCount: number
  values: SettingValues
}

export interface RowSummary {
  row: number
  logo: string | null
  layoutColor: string | null
  dealerName: string | null
}

export interface SettingChange {
  key: string
  value?: string | null
  setBits?: string
  clearBits?: string
}

export interface SaveResponse {
  changedKeys: string[]
  settings: SettingsResponse
}
