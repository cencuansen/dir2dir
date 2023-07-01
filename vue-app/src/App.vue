<script setup lang="ts">
import { ref, toRaw, nextTick } from "vue"
import { ElRow, ElCol, ElButton, ElInput, ElTable, ElTableColumn, ElMessage } from "element-plus"
import { Search } from '@element-plus/icons-vue'
import MessageDto from "./types/MessageDto"
import DirItemDto from "./types/DirItemDto"
import MoveProgressDto from "./types/MoveProgressDto"
import {
  MESSAGE, DIR_MOVE_ERROR, DIR_MOVE_PROGRESS, DIR_MOVE_OK
} from "./Constants"

const hostObject = window.chrome.webview.hostObjects
const sourcePath = ref("")
const sourceDirs = ref<DirItemDto[]>([])
const selectedDirs = ref<DirItemDto[]>([])
async function openSource() {
  sourcePath.value = await hostObject.host.DirectorySelector()
  sourceDirs.value = JSON.parse(await hostObject.host.GetSubDirs(sourcePath.value))
}

const targetPath = ref("")
async function openTarget() {
  targetPath.value = await hostObject.host.DirectorySelector()
}

let isAllSelected = ref(false)
let isBackuped = false
let sourceDirsBackup: DirItemDto[] = []
const keyword = ref("")
const tableRef = ref()
function currentSelection(item: DirItemDto) {
  if (item.isHidden || item.isLink || item.isReadOnly) {
    return
  }
  const existe = selectedDirs.value.find(d => d.name === item.name)
  if (existe) {
    selectedDirs.value = selectedDirs.value.filter(d => d.name !== item.name)
    tableRef?.value.toggleRowSelection(item, false)
    isAllSelected.value = false
  } else {
    selectedDirs.value.unshift(item)
    tableRef?.value.toggleRowSelection(item, true)
  }
}

async function search() {
  const trimedKeyword = keyword.value.trim()
  if (trimedKeyword) {
    if (!isBackuped) {
      isBackuped = true
      sourceDirsBackup = JSON.parse(JSON.stringify(toRaw(sourceDirs.value)))
    }
    sourceDirs.value = sourceDirsBackup.filter((d: DirItemDto) => new RegExp(trimedKeyword, 'i').test(d.name))
  } else {
    sourceDirs.value = JSON.parse(JSON.stringify(sourceDirsBackup))
    sourceDirsBackup = []
    isBackuped = false
  }
  await nextTick()
  selectedDirs.value.forEach(sel => {
    const canSel = sourceDirs.value.find(s => s.name === sel.name)
    if (canSel?.name) {
      tableRef?.value.toggleRowSelection(canSel, true)
    }
  })
}

function dirAttr(row: DirItemDto) {
  let attrs: string[] = []
  if (row.isLink) {
    attrs.push("链接")
  }
  if (row.isHidden) {
    attrs.push("隐藏")
  }
  if (row.isReadOnly) {
    attrs.push("只读")
  }
  return attrs.join(", ")
}

const dirSizeMap = ref<Map<string, number>>(new Map())
async function calcSize(row: DirItemDto) {
  const data = JSON.parse(await hostObject.host.CalcDirSize(row.fullPath))
  dirSizeMap.value.set(data.fullPath, data.size)
}

function sizeToBgColor(value: number): string {
  const maxValue = 1024 * 1024 * 1024 * 15
  const percentage = value / maxValue
  const num = Math.round(255 * (1 - percentage))
  const color = `rgb(255, ${num}, ${num})`
  return color
}
function sizeToTextColor(value: number): string {
  const maxValue = 1024 * 1024 * 1024
  const percentage = value / maxValue
  const num = Math.round(255 * percentage)
  const color = `rgb(${num}, ${num}, ${num})`
  return color
}
type CellStyle = {
  row: DirItemDto,
  column: any,
  rowIndex: number,
  columnIndex: number
}
function cellStyle({ row, column, }: CellStyle) {
  if (column.property === "size") {
    return {
      backgroundColor: sizeToBgColor(dirSizeMap.value.get(row.fullPath) ?? 0),
      color: sizeToTextColor(dirSizeMap.value.get(row.fullPath) ?? 0)
    }
  }

}

function getDirSize(row: DirItemDto) {
  if (dirSizeMap.value.has(row.fullPath)) {
    const size: number = dirSizeMap.value.get(row.fullPath) ?? 0
    if (size < 1024) {
      return `${size} bytes`
    } else if (size < 1024 * 1024) {
      return `${(size / 1024).toFixed(2)} KB`
    } else if (size < 1024 * 1024 * 1024) {
      return `${(size / 1024 / 1024).toFixed(2)} MB`
    } else if (size < 1024 * 1024 * 1024 * 1024) {
      return `${(size / 1024 / 1024 / 1024).toFixed(2)} GB`
    } else {
      return `${(size / 1024 / 1024 / 1024 / 1024).toFixed(2)} TB`
    }
  }
  return "0"
}

function selectCheckboxOne(_: DirItemDto[], sel: DirItemDto) {
  const exist = selectedDirs.value.find(ex => sel.name === ex.name)
  if (exist) {
    isAllSelected.value = false
    selectedDirs.value = selectedDirs.value.filter(s => s.name !== sel.name)
    tableRef?.value.data.filter((d: DirItemDto) => d.name === sel.name).forEach((d: DirItemDto) => tableRef?.value.toggleRowSelection(d, false))
  } else {
    selectedDirs.value.unshift(sel)
    tableRef?.value.data.filter((d: DirItemDto) => d.name === sel.name).forEach((d: DirItemDto) => tableRef?.value.toggleRowSelection(d, true))
  }
}

function toggelSelectAll() {
  isAllSelected.value ? unselectAll() : selectAll()
}

function selectAll() {
  isAllSelected.value = true
  selectedDirs.value = tableRef?.value.data.filter((d: DirItemDto) => !(d.isHidden || d.isLink || d.isReadOnly))
  tableRef?.value.data.forEach((d: DirItemDto) => {
    if (d.isHidden || d.isLink || d.isReadOnly) {
      tableRef?.value.toggleRowSelection(d, false)
    } else {
      tableRef?.value.toggleRowSelection(d, true)
    }
  })
}

function unselectAll() {
  isAllSelected.value = false
  selectedDirs.value = []
  tableRef?.value.data.forEach((d: DirItemDto) => tableRef?.value.toggleRowSelection(d, false))
}

async function moveButton() {
  if (selectedDirs.value.length === 0) {
    ElMessage("请勾选需要迁移的文件夹")
    return
  }
  if (targetPath.value === "") {
    ElMessage("目标文件夹不能为空")
    return
  }
  const selectedPaths = selectedDirs.value.map(d => d.fullPath)
  hostObject.host.MoveDirs(JSON.stringify({
    froms: selectedPaths,
    to: targetPath.value
  }))
}

const moveProgress = ref<MoveProgressDto[]>([])
const moveErrors = ref<string[]>([])

window.chrome.webview?.addEventListener(MESSAGE, (event) => {
  const data: MessageDto = event.data as MessageDto
  switch (data.name) {
    case DIR_MOVE_PROGRESS:
      moveProgress.value.push(data.data)
      break
    case DIR_MOVE_ERROR:
      moveErrors.value.push(data.data)
      break
    case DIR_MOVE_OK:
      ElMessage("文件移动成功")
      break
    default:
      break
  }
})
</script>

<template>
  <el-row class="row" :gutter="10">
    <el-col class="left" :span="4">
      <el-button @click="openSource">原始文件夹</el-button>
    </el-col>
    <el-col class="right" :span="20">
      <el-input v-model="sourcePath"></el-input>
    </el-col>
  </el-row>
  <el-row class="row" :gutter="10">
    <el-col class="left" :span="4">
      <el-button @click="openTarget">目标文件夹</el-button>
    </el-col>
    <el-col class="right" :span="20">
      <el-input v-model="targetPath"></el-input>
    </el-col>
  </el-row>
  <el-row class="row">
    <el-col class="right" :span="24">
      <el-input v-model="keyword" @input="search" :prefix-icon="Search" placeholder="关键字搜索"
        :disabled="sourceDirs.length === 0 && keyword === ''" :clearable="true"></el-input>
    </el-col>
  </el-row>
  <el-table ref="tableRef" @row-click="currentSelection" @select-all="toggelSelectAll" @select="selectCheckboxOne"
    :data="sourceDirs" max-height="400" empty-text="空空如也" :border="true" :cell-style="cellStyle">
    <el-table-column type="selection" width="55"
      :selectable="(row: DirItemDto) => !row.isLink && row.fullPath !== targetPath && !row.isHidden && !row.isReadOnly" />
    <el-table-column type="index" label="序号" width="70" :index="(index: number) => index + 1" align="center" />
    <el-table-column prop="name" label="名称" width="200" :resizable="true" :show-overflow-tooltip="true" />
    <el-table-column prop="fullPath" label="完整路径" :show-overflow-tooltip="true" />
    <el-table-column label="属性" width="100" align="center">
      <template #default="scope">
        <el-text v-text="dirAttr(scope.row)"></el-text>
      </template>
    </el-table-column>
    <el-table-column prop="size" label="大小" width="100">
      <template #default="scope">
        <el-text v-text="getDirSize(scope.row)"
          :style="{ color: sizeToTextColor(dirSizeMap.get(scope.row.fullPath) ?? 0) }"></el-text>
      </template>
    </el-table-column>
    <el-table-column label="操作" width="100">
      <template #default="scope">
        <el-button @click.stop.prevent="calcSize(scope.row)">计算大小</el-button>
      </template>
    </el-table-column>
  </el-table>
  <el-row :gutter="10">
    <el-col :span="2">
      <el-button @click="selectAll" :disabled="isAllSelected || sourceDirs.length === 0">全选</el-button>
    </el-col>
    <el-col :span="2">
      <el-button @click="unselectAll" :disabled="selectedDirs.length === 0">清空</el-button>
    </el-col>
    <el-col :span="2">
      <el-button @click="moveButton" :disabled="selectedDirs.length === 0 || targetPath === ''">迁移</el-button>
    </el-col>
    <el-col :span="10" align="left" class="select-info">
      <el-text>共 {{ isBackuped ? sourceDirsBackup.length : sourceDirs.length }} 个，当前页 {{ sourceDirs.length }} 个，已选 {{
        selectedDirs.length }} 个</el-text>
    </el-col>
  </el-row>
  <el-row class="selected-area">
    <el-text v-for=" selected in selectedDirs" :key="selected.fullPath" :type="'primary'" align="left">
      {{ selected.name }}
    </el-text>
  </el-row>
  <el-row :gutter="10" v-for=" progress in moveProgress" :key="progress.percentage" class="move-progress">
    <el-col :span="2" align="left">
      <el-text>
        {{ (progress?.percentage ?? 0) * 100 }} %
      </el-text>
    </el-col>
    <el-col :span="22" align="left">
      <el-text>
        {{ progress?.path }}
      </el-text>
    </el-col>
  </el-row>
  <el-row class="error-area">
    <el-row v-for=" error in moveErrors " :key="error">
      <el-text :type="'error'" align="left">
        {{ error }}
      </el-text>
    </el-row>
  </el-row>
</template>

<style scoped>
.el-button {
  width: 100%;
}

.el-row {
  margin-bottom: 10px;
}

.el-table {
  margin-bottom: 10px;
  user-select: none;
}

.error-area,
.move-progress {
  max-height: 250px;
  overflow-y: auto;
}

.selected-area {
  display: flex;
  flex-wrap: nowrap;
  overflow-x: auto;
}

.selected-area span {
  margin: 5px;
  white-space: nowrap;
  color: #fff;
  padding: 3px 5px;
  border-radius: 5px;
  background-color: #aaa;
}

.select-info {
  display: flex !important;
  align-items: center;
}
</style>
