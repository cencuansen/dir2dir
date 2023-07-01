type DirItemDto = {
    name: string,
    cTime: Date,
    mTime: Date,
    fullPath: string,
    isFile: boolean,
    isDir: boolean,
    isLink: boolean,
    isHidden: boolean,
    isReadOnly: boolean,
    invisible: boolean
}

export default DirItemDto