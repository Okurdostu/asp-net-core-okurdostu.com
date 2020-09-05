const mb = 5;
const acceptableFileSize = 1048576 * mb;
function IsSizeAcceptable(size){
    return size < acceptableFileSize;
}