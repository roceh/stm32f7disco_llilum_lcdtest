#include "helpers.h"
#include "ff_gen_drv.h"
#include "sd_diskio.h"


Diskio_drvTypeDef* GetSDDriver()
{
	return &SD_Driver;
}

FATFS* FATFSAlloc()
{
	return (FATFS*) calloc(sizeof(FATFS), 1);
}

void FATFSFree(FATFS* fatfs)
{
	free(fatfs);
}

FILINFO* FILINFOAlloc()
{
	return (FILINFO*) calloc(sizeof(FILINFO), 1);
}

void FILINFOFree(FILINFO* filinfo)
{
	free(filinfo);
}

FIL* FILAlloc()
{
	return (FIL*) calloc(sizeof(FIL), 1);
}

void FILFree(FIL* fil)
{
	free(fil);
}

uint32_t GetFileSize(FIL* fil)
{
	return f_size(fil);
}

uint32_t GetFilePosition(FIL* fil)
{
	return f_tell(fil);
}

