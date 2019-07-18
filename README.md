# RemoteDev

RemoteDev is alpha at best. So your milesage may very, but here's how you can use it today

## Usage

1. Configure RemoteDev to point to another folder, a network drive, FTP, FTPS, SFTP, etc.
1. Clone a git repo both locally and remotely
1. Run RemoteDev on your local machine
1. Changes you make locally are reflected on the remote
1. Sit back and enjoy your productivity

## But why re-invent the wheel??!!

There are tons of utilities that synchronize files: rsync, FileZilla, WinSCP, etc. Why create another one? I'll tell you:

1. Real-time, automatic updates
1. Gitignore file filtering

That last one is a biggie. A lot of other solutions require you to add a ton of customized filters to try and configure your sync utility to try and pick and choose which files to actually synchronize. But isn't that exactly why the gitignore exists? RemoteDev looks at your gitignore to know which files to care about.

## Example usage:

Currently the tool can sync to another directory (the `sync-dir` command) or to an SFTP location (`sync-sftp`). Once you run the command, the file monitor will start and any changes to any files in that local directory (or any of its subfolders) that are not excluded by your gitignore file will be sent to the remote that you specify (another folder or an SFTP location).

### Example directory sync-ing

```
RemoveDev sync-dir
  --local C:\\dev\workplace\my-project\src
  --remote C:\\dev\workplace\my-project-clone\src
```

### Example SFTP sync-ing

```
RemoveDev sync-sftp
  --local C:\\dev\workplace\my-project\src
  --host my-sftp-box.example.com
  --working-directory /workplace/my-project/src
  --user my_user_name
```

The SFTP mode will prompt you for your password. You can also specify the password as a command line flag (if you feel like living on the edge):

```-- password ThisPasswordIsAwful```
