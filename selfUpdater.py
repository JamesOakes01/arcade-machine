import os
import git
from git import Repo

repo_path = os.path.dirname(os.path.abspath(__file__))


def update_from_git(repo_path, branch="main"):
    """Updates the code from a Git repository."""

    try:
        repo = Repo(repo_path)
        if repo.is_dirty():
            print("Repository has uncommitted changes. Please commit or stash them before updating.")
            return False

        origin = repo.remotes.origin
        origin.fetch()

        if repo.active_branch.name != branch:
            print(f"Switching to branch {branch}")
            repo.git.checkout(branch)

        print("Pulling changes from remote...")
        origin.pull()

        print("Update successful.")
        return True

    except git.exc.GitCommandError as e:
        print(f"Error updating from Git: {e}")
        return False

if __name__ == "__main__":
    repo_path = "."  # Replace with the actual path to your repository
    update_from_git(repo_path)

