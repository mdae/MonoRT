#include <linux/module.h>
#include <linux/fs.h>
#include <linux/sched.h>
#include <linux/fcntl.h>
#include <asm/uaccess.h>

#define SPKR_MAJOR 241
MODULE_LICENSE("GPL");

static int driver_open( struct inode *geraetedatei, struct file *instanz )
{
	return 0;
}


static ssize_t driver_read(struct file *filp,
			char *buffer,    /* The buffer to fill with data */
			size_t length,   /* The length of the buffer     */
			loff_t *offset)  /* Our offset in the file       */
{
	return (ssize_t) length;
}


static ssize_t driver_write( struct file *instanz, __user const char *user,
    size_t count, loff_t *offs)
{
 	printk("driver_write called. flushing the CPU cache.\n");
	#if (defined(__i386__))
	// flushing the CPU caches on x86 systems
	asm volatile("wbinvd":::"memory");
	#elif (defined (__arm__))
	// Invalidate Entire Instruction Cache. Also flushes the branch target cache
	__asm__ __volatile__ ("mcr p15, 0, %0, c7, c5, 0" : : "r" (0));
	// Clean and Invalidate Entire Data Cache
	__asm__ __volatile__ ("mcr p15, 0, %0, c7, c14, 0" : : "r" (0));
	#else
	#error Unknown Architecture!
	#endif
	return (ssize_t) count;
}

static struct file_operations fops = {
	.owner=THIS_MODULE,
	.write=driver_write,
	.open=driver_open,
	.read=driver_read,
};

static int __init driver_init(void)
{
	printk("driver_init called\n");
	if(  register_chrdev(SPKR_MAJOR,"testdev2",&fops)!=0) {
		printk("error register_chrdev\n");
		return -EIO;		
	}
	return 0;
}

static void __exit driver_exit(void)
{
	printk("driver_exit gerufen\n");
	unregister_chrdev(SPKR_MAJOR,"testdev2");
}

module_init( driver_init );
module_exit( driver_exit );

